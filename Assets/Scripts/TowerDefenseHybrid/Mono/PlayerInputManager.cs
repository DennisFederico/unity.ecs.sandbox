using TowerDefenseBase.Helpers;
using TowerDefenseHybrid.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Authoring;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TowerDefenseHybrid.Mono {
    public class PlayerInputManager : MonoBehaviour {

        public static PlayerInputManager Instance;
        public event System.Action<CellPlacementData> OnGridCellCandidateChange;

        [SerializeField] private Camera mainCamera;
        [SerializeField] private InputAction updateHoverPositionAction;
        [SerializeField] private InputAction placeBuildingAction;
        [SerializeField] private InputAction destroyBuildingAction;
        [SerializeField] private InputAction rotateBuildingGhostAction;
        [SerializeField] private InputAction selectSpecificBuildingAction;
        [SerializeField] private InputAction selectNextBuildingAction;
        [SerializeField] private LayerMask buildableLayerMask;
        [SerializeField] private PhysicsCategoryTags buildingShapeTag;
        [SerializeField] private GameObject buildingPrefab;
        [SerializeField] private int numBuildingTypes;
        private World _world;
        private Entity _placeBuildingBufferEntity;
        private Entity _destroyBuildingBufferEntity;


        private CellPlacementData _candidateCellPlacementData;

        public struct CellPlacementData : System.IEquatable<CellPlacementData> {
            public int2 GridPosition;
            public bool IsBuildable;
            public bool IsOccupied;
            public int BuildingIndex;
            public PlacementFacing PlacementFacing;
            public Vector3 CellCenterWorldPosition;
            public Vector3 WorldPosition;

            public void Invalidate() {
                GridPosition = new int2(-1, -1);
                IsOccupied = true;
            }

            public bool IsValid => GridPosition is { x: >= 0, y: >= 0 };

            public bool Equals(CellPlacementData other) {
                return GridPosition.Equals(other.GridPosition) && IsOccupied == other.IsOccupied;
            }

            public override bool Equals(object obj) {
                return obj is CellPlacementData other && Equals(other);
            }

            public override int GetHashCode() {
                return System.HashCode.Combine(GridPosition, IsOccupied);
            }
        }

        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                throw new System.Exception("An instance of this singleton already exists.");
            }

            Instance = this;

            _candidateCellPlacementData = new CellPlacementData();
            _candidateCellPlacementData.Invalidate();

            if (mainCamera == null) mainCamera = Camera.main;
        }

        private void OnEnable() {
            mainCamera = mainCamera == null ? Camera.main : mainCamera;
            updateHoverPositionAction.performed += OnUpdateHoverPositionAction;
            updateHoverPositionAction.Enable();
            placeBuildingAction.started += OnPlaceBuildingAction;
            placeBuildingAction.Enable();
            destroyBuildingAction.started += OnDestroyBuildingAction;
            destroyBuildingAction.Enable();
            rotateBuildingGhostAction.started += OnRotateBuildingGhostAction;
            rotateBuildingGhostAction.Enable();
            selectSpecificBuildingAction.performed += OnSelectSpecificBuildingGhostAction;
            selectSpecificBuildingAction.Enable();
            selectNextBuildingAction.performed += OnSelectNextBuildingGhostAction;
            selectNextBuildingAction.Enable();
            _world = World.DefaultGameObjectInjectionWorld;
        }

        private void OnDisable() {
            updateHoverPositionAction.Disable();
            updateHoverPositionAction.performed -= OnPlaceBuildingAction;
            placeBuildingAction.Disable();
            placeBuildingAction.started -= OnPlaceBuildingAction;
            destroyBuildingAction.Disable();
            destroyBuildingAction.started -= OnDestroyBuildingAction;
            rotateBuildingGhostAction.Disable();
            rotateBuildingGhostAction.started -= OnRotateBuildingGhostAction;
            selectSpecificBuildingAction.Disable();
            selectSpecificBuildingAction.performed -= OnSelectSpecificBuildingGhostAction;
            selectNextBuildingAction.Disable();
            selectNextBuildingAction.performed -= OnSelectNextBuildingGhostAction;
            
            if (_world.IsCreated) {
                if (_world.EntityManager.Exists(_placeBuildingBufferEntity)) {
                    _world.EntityManager.DestroyEntity(_placeBuildingBufferEntity);
                }
                if (_world.EntityManager.Exists(_destroyBuildingBufferEntity)) {
                    _world.EntityManager.DestroyEntity(_destroyBuildingBufferEntity);
                }
            }
        }

        private void OnUpdateHoverPositionAction(InputAction.CallbackContext ctx) {
            //RayCast when mouse moves to capture the status of the cell, if it is
            //a different cell (position) than the previous one, raise an event
            var ray = mainCamera.ScreenPointToRay(ctx.ReadValue<Vector2>());

            if (!Physics.Raycast(ray, out var hit, mainCamera.farClipPlane, buildableLayerMask)) {
                //If the RayCast does not hit the grid (a buildable place), invalidate the candidate grid cell data
                if (_candidateCellPlacementData.IsValid) {
                    _candidateCellPlacementData.Invalidate();
                    OnGridCellCandidateChange?.Invoke(_candidateCellPlacementData);
                }
            } else {
                if (GridManager.Instance.Grid.GetCellGridWorldPositions(hit.point, out var cellCenter, out var gridPosition, true)) {
                    //If it is a valid grid cell and it is grid position is different from the previous one, raise the event
                    if (!_candidateCellPlacementData.GridPosition.Equals(gridPosition)) {
                        var gridObject = GridManager.Instance.Grid.GetGridObject(gridPosition);
                        var gridSelectionChangeEvent = new CellPlacementData {
                            GridPosition = gridObject.GridPosition,
                            BuildingIndex = _candidateCellPlacementData.BuildingIndex,
                            PlacementFacing = _candidateCellPlacementData.PlacementFacing,
                            IsOccupied = gridObject.IsOccupied(),
                            IsBuildable = gridObject.IsBuildable,
                            CellCenterWorldPosition = cellCenter,
                            WorldPosition = hit.point
                        };
                        _candidateCellPlacementData = gridSelectionChangeEvent;
                        OnGridCellCandidateChange?.Invoke(_candidateCellPlacementData);
                    } else {
                        //Only update the mouse world position if the grid position is the same
                        _candidateCellPlacementData.WorldPosition = hit.point;
                    }
                }
            }
        }

        private void OnPlaceBuildingAction(InputAction.CallbackContext ctx) {
            if (_candidateCellPlacementData is not { IsBuildable: true, IsOccupied: false } ||
                _candidateCellPlacementData.BuildingIndex == 0) return;
            
            //Send a "command" to the ECS side to place the building
            RequestEcsBuildingPlacement(_candidateCellPlacementData);

            //Update the Grid
            var gridObject = GridManager.Instance.Grid.GetGridObject(_candidateCellPlacementData.GridPosition);
            gridObject.Name = buildingPrefab.name;
            GridManager.Instance.Grid.SetGridObject(gridObject.GridPosition, gridObject);
                
            //Send and event to update the UI
            _candidateCellPlacementData.IsOccupied = true;
            OnGridCellCandidateChange?.Invoke(_candidateCellPlacementData);
        }

        private void OnDestroyBuildingAction(InputAction.CallbackContext ctx) {
            if (!_candidateCellPlacementData.IsOccupied) return;
            //Send a "command" to the ECS side to destroy the building
            RequestEcsBuildingDestroy(_candidateCellPlacementData);
                
            //Update the Grid
            var gridObject = GridManager.Instance.Grid.GetGridObject(_candidateCellPlacementData.GridPosition);
            gridObject.Name = null;
            GridManager.Instance.Grid.SetGridObject(gridObject.GridPosition, gridObject);
                
            //Send and event to update the UI
            _candidateCellPlacementData.IsOccupied = false;
            OnGridCellCandidateChange?.Invoke(_candidateCellPlacementData);
        }

        private void OnRotateBuildingGhostAction(InputAction.CallbackContext ctx) {
            var facingChange = (int)ctx.ReadValue<float>();
            _candidateCellPlacementData.PlacementFacing = _candidateCellPlacementData.PlacementFacing.ChangeBy(facingChange);
            OnGridCellCandidateChange?.Invoke(_candidateCellPlacementData);
        }
        
        private void OnSelectSpecificBuildingGhostAction(InputAction.CallbackContext obj) {
            _candidateCellPlacementData.BuildingIndex = (int)obj.ReadValue<float>();
            OnGridCellCandidateChange?.Invoke(_candidateCellPlacementData);
        }
        
        private void OnSelectNextBuildingGhostAction(InputAction.CallbackContext obj) {
            var delta = (int)obj.ReadValue<float>(); //value clamped between -1 and 1
            var buildingIndex = _candidateCellPlacementData.BuildingIndex + delta;
            buildingIndex = buildingIndex < 0 ? numBuildingTypes : buildingIndex > numBuildingTypes ? 0 : buildingIndex;
            _candidateCellPlacementData.BuildingIndex = buildingIndex;
            OnGridCellCandidateChange?.Invoke(_candidateCellPlacementData);
        }

        private void RequestEcsBuildingPlacement(CellPlacementData cellData) {
            if (_world.IsCreated && !_world.EntityManager.Exists(_placeBuildingBufferEntity)) {
                _placeBuildingBufferEntity = _world.EntityManager.CreateEntity();
                _world.EntityManager.AddBuffer<BuildingPlacementByTransform>(_placeBuildingBufferEntity);
            }

            _world.EntityManager.GetBuffer<BuildingPlacementByTransform>(_placeBuildingBufferEntity)
                .Add(new BuildingPlacementByTransform() {
                    BuildingId = (sbyte) cellData.BuildingIndex,
                    Position = cellData.CellCenterWorldPosition,
                    Rotation = cellData.PlacementFacing.Rotation()
                });
        }
        
        private void RequestEcsBuildingDestroy(CellPlacementData cellData) {
            if (_world.IsCreated && !_world.EntityManager.Exists(_destroyBuildingBufferEntity)) {
                _destroyBuildingBufferEntity = _world.EntityManager.CreateEntity();
                _world.EntityManager.AddBuffer<BuildingDestroyByTransform>(_destroyBuildingBufferEntity);
            }

            _world.EntityManager.GetBuffer<BuildingDestroyByTransform>(_destroyBuildingBufferEntity)
                .Add(new BuildingDestroyByTransform() {
                    Position = cellData.CellCenterWorldPosition,
                    ShapeTag = buildingShapeTag
                });
        }
    }
}