using TowerDefenseBase.Helpers;
using TowerDefenseBase.Input;
using TowerDefenseEcs.Components;
using Unity.Entities;
using Unity.Physics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TowerDefenseEcs.Systems {
    public partial class BuildingInputSystem : SystemBase {

        //TODO: Enable/Disable System on MonoBehaviour Create/Destroy or Scene Load/Unload
        private TowerDefenseBaseInput _input;
        private TowerDefenseBaseInput.PlayerActionsActions _playerActions;

        //TODO: Should we have an entity with the latest Input state that can be read by other system?
        private bool _processNewInputState;
        private struct InputState {
            public Vector2 MousePosition;
            public bool UpdateMousePosition;
            public bool PlaceStandardTurret;
            public bool PlaceFreezeTurret;
            public bool RotateTurret;
            public bool DestroyTurret;

            //TODO... Just for debug. Remove this
            public override string ToString() {
                return $"InputState: \n \tMousePosition: {MousePosition} \n \tPlaceStandardTurret: {PlaceStandardTurret} \n \tPlaceFreezeTurret: {PlaceFreezeTurret} \n \tRotateTurret: {RotateTurret} \n \tDestroyTurret: {DestroyTurret}";
            }
        }
        private InputState _inputState;
        
        //TODO should we put a reference in the config data to the main camera?
        private Camera _mainCamera;
        private BuildingSystemConfigData _buildingSystemConfigData;
        private PlacementFacing _placementFacing;
        private CollisionFilter _placeBuildingCollisionFilter;
        private CollisionFilter _destroyBuildingCollisionFilter;

        protected override void OnCreate() {
            //TODO Should we handle the system to be enabled/disable from Mono on Scene Load/Unload?
            //Mainly because of the InputSystem
            //Enabled = false;
            
            RequireForUpdate<BuildingSystemConfigData>();
            CreateBufferIfRequired<PlaceBuildingData>();
            CreateBufferIfRequired<DestroyBuildingData>();
            CreateBufferIfRequired<GhostBuildingData>();
        }
        
        protected override void OnStartRunning() {
            _mainCamera = _mainCamera == null ? Camera.main : _mainCamera;
            
            //PREPARE THE INPUT SYSTEM
            _input = new TowerDefenseBaseInput();
            _playerActions = _input.PlayerActions;
            
            _playerActions.MouseMove.performed += OnUpdateMousePosition;
            _playerActions.PlaceStandardTurret.started += OnPlaceStandardTurret;
            _playerActions.PlaceFreezeTurret.started += OnPlaceFreezeTurret;
            _playerActions.RotateTurret.started += OnRotateTurret;
            _playerActions.DestroyTurret.started += OnDestroyTurret;
            _playerActions.Enable();
            
            //PREPARE THE COLLISION FILTERS
            _buildingSystemConfigData = SystemAPI.GetSingleton<BuildingSystemConfigData>();
            _placeBuildingCollisionFilter = new CollisionFilter() {
                BelongsTo = _buildingSystemConfigData.InputSystemTag.Value,
                CollidesWith = _buildingSystemConfigData.TerrainTag.Value,
                GroupIndex = 0
            };
            _destroyBuildingCollisionFilter = new CollisionFilter() {
                BelongsTo = _buildingSystemConfigData.InputSystemTag.Value,
                CollidesWith = _buildingSystemConfigData.TurretTag.Value,
                GroupIndex = 0
            };
        }
        
        protected override void OnStopRunning() {
            _playerActions.MouseMove.performed -= OnUpdateMousePosition;
            _playerActions.PlaceStandardTurret.started -= OnPlaceStandardTurret;
            _playerActions.PlaceFreezeTurret.started -= OnPlaceFreezeTurret;
            _playerActions.RotateTurret.started -= OnRotateTurret;
            _playerActions.DestroyTurret.started -= OnDestroyTurret;
            _playerActions.Disable();
        }
        
        private void OnUpdateMousePosition(InputAction.CallbackContext ctx) {
            _processNewInputState = true;
            _inputState = new InputState() {
                MousePosition = ctx.ReadValue<Vector2>(),
                UpdateMousePosition = true
            };
        }

        private void OnPlaceStandardTurret(InputAction.CallbackContext ctx) {
            _processNewInputState = true;
            _inputState = new InputState() {
                MousePosition = ctx.ReadValue<Vector2>(),
                PlaceStandardTurret = true
            };
        }
        
        private void OnPlaceFreezeTurret(InputAction.CallbackContext ctx) {
            _processNewInputState = true;
            _inputState = new InputState() {
                MousePosition = ctx.ReadValue<Vector2>(),
                PlaceFreezeTurret = true
            };
        }
        
        private void OnRotateTurret(InputAction.CallbackContext ctx) {
            _processNewInputState = true;
            _inputState = new InputState() {
                MousePosition = ctx.ReadValue<Vector2>(),
                RotateTurret = true
            };
        }
        
        private void OnDestroyTurret(InputAction.CallbackContext ctx) {
            _processNewInputState = true;
            _inputState = new InputState() {
                MousePosition = ctx.ReadValue<Vector2>(),
                DestroyTurret = true
            };
        }

        protected override void OnUpdate() {
            if (!_processNewInputState) return;

            if (_inputState.UpdateMousePosition) {
                ProcessUpdateMousePositionInput();
            }
            if (_inputState.PlaceStandardTurret || _inputState.PlaceFreezeTurret) {
                ProcessPlaceBuildingInput();
            }
            if (_inputState.DestroyTurret) {
                ProcessDestroyBuildingInput();
            }
            if (_inputState.RotateTurret) {
                _placementFacing = _placementFacing.Next();
                ProcessUpdateMousePositionInput();
            }
            _processNewInputState = false;
        }

        private void CreateBufferIfRequired<T>() where T : unmanaged, IBufferElementData {
            if (SystemAPI.TryGetSingletonBuffer(out DynamicBuffer<T> _)) return;
            EntityManager.CreateSingletonBuffer<T>();
        }

        private void ProcessUpdateMousePositionInput() {
            var screenPointToRay = _mainCamera.ScreenPointToRay(_inputState.MousePosition);
            var rayInput = new RaycastInput() {
                Start = screenPointToRay.origin,
                End = screenPointToRay.GetPoint(_mainCamera.farClipPlane),
                Filter = _placeBuildingCollisionFilter
            };
            var buffer = SystemAPI.GetSingletonBuffer<GhostBuildingData>();
            buffer.Add(new GhostBuildingData() {
                RayInput = rayInput,
                Rotation = _placementFacing.Rotation(),
                BuildingIndex = 0,
                ObstacleLayers = _buildingSystemConfigData.PlacingObstacles
            });
        }

        private void ProcessPlaceBuildingInput() {
            var screenPointToRay = _mainCamera.ScreenPointToRay(_inputState.MousePosition);
            var rayInput = new RaycastInput() {
                Start = screenPointToRay.origin,
                End = screenPointToRay.GetPoint(_mainCamera.farClipPlane),
                Filter = _placeBuildingCollisionFilter
            };
            var buffer = SystemAPI.GetSingletonBuffer<PlaceBuildingData>();
            buffer.Add(new PlaceBuildingData() {
                RayInput = rayInput,
                Rotation = _placementFacing.Rotation(),
                BuildingIndex = _inputState.PlaceFreezeTurret ? 1 : 0,
                ObstacleLayers = _buildingSystemConfigData.PlacingObstacles
            });
        }

        private void ProcessDestroyBuildingInput() {
            var screenPointToRay = _mainCamera.ScreenPointToRay(_inputState.MousePosition);
            var rayInput = new RaycastInput() {
                Start = screenPointToRay.origin,
                End = screenPointToRay.GetPoint(_mainCamera.farClipPlane),
                Filter = _destroyBuildingCollisionFilter
            };
            var buffer = SystemAPI.GetSingletonBuffer<DestroyBuildingData>();
            buffer.Add(new DestroyBuildingData() {
                Value = rayInput
            });
        }
    }
}
