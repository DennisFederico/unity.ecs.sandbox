using TowerDefenseEcs.Components;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Authoring;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace TowerDefenseEcs.Mono {
    public class PlayerInputManager : MonoBehaviour {

        [SerializeField] private InputAction mouseMoveAction;
        [SerializeField] private InputAction placeStandardTurretAction;
        [SerializeField] private InputAction placeFreezeTurretAction;
        [SerializeField] private InputAction destroyTurretAction;
        [SerializeField] private InputAction rotateGhostAction;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private PhysicsCategoryTags inputSystemTag;
        [SerializeField] private PhysicsCategoryTags terrainTag;
        [SerializeField] private PhysicsCategoryTags turretTag;
        private Entity _placeBuildingBufferEntity;
        private Entity _destroyBuildingBufferEntity;
        private World _world;
        private CollisionFilter _placeTurretCollisionFilter;
        private CollisionFilter _destroyTurretCollisionFilter;

        private void Awake() {
            _placeTurretCollisionFilter = new CollisionFilter() {
                BelongsTo = inputSystemTag.Value,
                CollidesWith = terrainTag.Value,
                GroupIndex = 0
            };
            _destroyTurretCollisionFilter = new CollisionFilter() {
                BelongsTo = inputSystemTag.Value,
                CollidesWith = turretTag.Value,
                GroupIndex = 0
            };
        }

        private void OnEnable() {
            placeStandardTurretAction.started += OnPlaceStandardTurretMouse;
            placeStandardTurretAction.Enable();
            placeFreezeTurretAction.started += OnPlaceFreezeTurretMouse;
            placeFreezeTurretAction.Enable();
            destroyTurretAction.performed += OnDestroyTurretAction;
            destroyTurretAction.Enable();

            mainCamera = mainCamera == null ? Camera.main : mainCamera;

            _world = World.DefaultGameObjectInjectionWorld;
        }

        private void OnPlaceStandardTurretMouse(InputAction.CallbackContext ctx) => OnMouseClicked(ctx);
        private void OnPlaceFreezeTurretMouse(InputAction.CallbackContext ctx) => OnMouseClicked(ctx, true);

        private void OnMouseClicked(InputAction.CallbackContext ctx, bool isRightClick = false) {
            var screenPos = ctx.ReadValue<Vector2>();
            var screenPointToRay = mainCamera.ScreenPointToRay(screenPos);


            if (_world.IsCreated && !_world.EntityManager.Exists(_placeBuildingBufferEntity)) {
                _placeBuildingBufferEntity = _world.EntityManager.CreateEntity();
                _world.EntityManager.AddBuffer<PlaceBuildingRayInputData>(_placeBuildingBufferEntity);
            }

            _world.EntityManager.GetBuffer<PlaceBuildingRayInputData>(_placeBuildingBufferEntity).Add(new PlaceBuildingRayInputData() {
                Value = new RaycastInput() {
                    Start = screenPointToRay.origin,
                    End = screenPointToRay.GetPoint(mainCamera.farClipPlane),
                    Filter = _placeTurretCollisionFilter
                },
                TowerIndex = isRightClick ? 1 : 0
            });
        }
        
        private void OnDestroyTurretAction(InputAction.CallbackContext ctx) {
            var screenPos = ctx.ReadValue<Vector2>();
            var screenPointToRay = mainCamera.ScreenPointToRay(screenPos);
            
            if (_world.IsCreated && !_world.EntityManager.Exists(_destroyBuildingBufferEntity)) {
                _destroyBuildingBufferEntity = _world.EntityManager.CreateEntity();
                _world.EntityManager.AddBuffer<DestroyBuildingRayInputData>(_destroyBuildingBufferEntity);
            }

            _world.EntityManager.GetBuffer<DestroyBuildingRayInputData>(_destroyBuildingBufferEntity).Add(new DestroyBuildingRayInputData() {
                Value = new RaycastInput() {
                    Start = screenPointToRay.origin,
                    End = screenPointToRay.GetPoint(mainCamera.farClipPlane),
                    Filter = _destroyTurretCollisionFilter
                }
            });
        }

        private void OnDisable() {
            placeStandardTurretAction.Disable();
            placeStandardTurretAction.started -= OnPlaceStandardTurretMouse;
            placeFreezeTurretAction.Disable();
            placeFreezeTurretAction.started -= OnPlaceFreezeTurretMouse;
            destroyTurretAction.Disable();
            destroyTurretAction.performed -= OnDestroyTurretAction;

            if (_world.IsCreated) {
                if (_world.EntityManager.Exists(_placeBuildingBufferEntity)) {
                    _world.EntityManager.DestroyEntity(_placeBuildingBufferEntity);
                }
                if (_world.EntityManager.Exists(_destroyBuildingBufferEntity)) {
                    _world.EntityManager.DestroyEntity(_destroyBuildingBufferEntity);
                }
            }
        }
    }
}