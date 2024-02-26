using System;
using TowerDefenseBase.Components;
using TowerDefenseBase.Helpers;
using TowerDefenseBase.Input;
using TowerDefenseEcs.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TowerDefenseEcs.Systems {
    public partial class BuildingInputSystem : SystemBase {

        //TODO: Enable/Disable System on MonoBehaviour Create/Destroy or Scene Load/Unload
        private TowerDefenseBaseInput _input;
        private TowerDefenseBaseInput.PlayerActionsActions _playerActions;

        //TODO: Should we have an entity with the latest Input state that can be read by other system?
        //TODO: Should add Rotation and Building Index as part of the state
        private bool _processNewInputState;
        private struct InputState {
            public Vector2 MousePosition;
            // public int BuildingIndex;
            // public quaternion Rotation;
            public InputAction Action;
            
            public enum InputAction {
                UpdateState,
                BuildAction,
                DestroyAction,
            }
        }
        private InputState _inputState;
        
        private Camera _mainCamera;
        private BuildingSystemConfigData _buildingSystemConfigData;
        private PlacementFacing _placementFacing;
        private int _currentBuildingIndex;
        private int _numBuildingTypes;
        private CollisionFilter _placeBuildingCollisionFilter;
        private CollisionFilter _destroyBuildingCollisionFilter;

        protected override void OnCreate() {
            //TODO Should we handle the system to be enabled/disable from Mono on Scene Load/Unload? Mainly because of the InputSystem
            //Enabled = false;
            RequireForUpdate<BuildingSystemConfigData>();
            RequireForUpdate<BuildingRegistryTag>();
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
            _playerActions.BuildTurret.started += OnBuildTurret;
            _playerActions.RotateTurret.started += OnRotateTurret;
            _playerActions.DestroyTurret.started += OnDestroyTurret;
            _playerActions.SelectTurret.performed += OnSelectTurret;
            _playerActions.SelectTurretScroll.performed += OnSelectTurretScroll; 
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
            
            //Dynamically get the number of building types
            var buildingRegistry = SystemAPI.GetSingletonEntity<BuildingRegistryTag>();
            _numBuildingTypes = SystemAPI.GetBuffer<BuildingsBufferElementData>(buildingRegistry).Length;
        }

        protected override void OnStopRunning() {
            _playerActions.MouseMove.performed -= OnUpdateMousePosition;
            _playerActions.BuildTurret.started -= OnBuildTurret;
            _playerActions.RotateTurret.started -= OnRotateTurret;
            _playerActions.DestroyTurret.started -= OnDestroyTurret;
            _playerActions.SelectTurret.performed -= OnSelectTurret;
            _playerActions.SelectTurretScroll.performed += OnSelectTurretScroll; 
            _playerActions.Disable();
        }

        private void OnUpdateMousePosition(InputAction.CallbackContext ctx) {
            _processNewInputState = true;
            _inputState = new InputState() {
                MousePosition = ctx.ReadValue<Vector2>(),
                Action = InputState.InputAction.UpdateState
            };
        }

        private void OnBuildTurret(InputAction.CallbackContext ctx) {
            _processNewInputState = true;
            _inputState = new InputState() {
                MousePosition = ctx.ReadValue<Vector2>(),
                Action = InputState.InputAction.BuildAction
            };
        }

        private void OnSelectTurret(InputAction.CallbackContext obj) {
            _processNewInputState = true;
            _currentBuildingIndex = (int)obj.ReadValue<float>();
            var lastPosition = _inputState.MousePosition;
            _inputState = new InputState() {
                MousePosition = lastPosition,
                Action = InputState.InputAction.UpdateState
            };
        }
        
        
        private void OnSelectTurretScroll(InputAction.CallbackContext obj) {
            _processNewInputState = true;
            var delta = (int)obj.ReadValue<float>(); //value clamped between -1 and 1
            //_currentBuildingIndex = math.clamp(delta + _currentBuildingIndex, 0, _numBuildingTypes);
            _currentBuildingIndex += delta;
            _currentBuildingIndex = _currentBuildingIndex < 0 ? _numBuildingTypes : _currentBuildingIndex > _numBuildingTypes ? 0 : _currentBuildingIndex;
            var lastPosition = _inputState.MousePosition;
            _inputState = new InputState() {
                MousePosition = lastPosition,
                Action = InputState.InputAction.UpdateState
            };
        }

        private void OnRotateTurret(InputAction.CallbackContext ctx) {
            _processNewInputState = true;
            var facingScroll = (int)ctx.ReadValue<float>();
            _placementFacing = _placementFacing.ChangeBy(facingScroll);
            var lastPosition = _inputState.MousePosition;
            _inputState = new InputState() {
                MousePosition = lastPosition,
                Action = InputState.InputAction.UpdateState
            };
        }


        private void OnDestroyTurret(InputAction.CallbackContext ctx) {
            _processNewInputState = true;
            _inputState = new InputState() {
                MousePosition = ctx.ReadValue<Vector2>(),
                Action = InputState.InputAction.DestroyAction
            };
        }

        protected override void OnUpdate() {
            if (!_processNewInputState) return;

            switch (_inputState.Action) {
                case InputState.InputAction.UpdateState:
                    ProcessUpdateMousePositionInput();
                    break;
                case InputState.InputAction.BuildAction:
                    ProcessPlaceBuildingInput();
                    break;
                case InputState.InputAction.DestroyAction:
                    ProcessDestroyBuildingInput();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
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
                BuildingIndex = _currentBuildingIndex -1,
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
                BuildingIndex = _currentBuildingIndex -1,
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
