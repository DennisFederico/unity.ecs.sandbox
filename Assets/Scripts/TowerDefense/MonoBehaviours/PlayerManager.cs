using TowerDefense.Components;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Authoring;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TowerDefense.MonoBehaviours {
    public class PlayerManager : MonoBehaviour {

        [SerializeField] private InputAction inputAction;
        [SerializeField] private Camera mainCamera;
        [SerializeField] private int towerIndex;
        [SerializeField] private PhysicsCategoryTags belongsToTag;
        [SerializeField] private PhysicsCategoryTags collidesWithTag;
        private Entity _entity;
        private World _world;
        
        private void OnEnable() {
            inputAction.started += OnMouseClicked;
            inputAction.Enable();
            
            mainCamera = mainCamera == null ? Camera.main : mainCamera;
            
            _world = World.DefaultGameObjectInjectionWorld;
        }

        private void OnMouseClicked(InputAction.CallbackContext ctx) {
            var screenPos = ctx.ReadValue<Vector2>();
            var screenPointToRay = mainCamera.ScreenPointToRay(screenPos);
            
            if (_world.IsCreated && !_world.EntityManager.Exists(_entity)) {
                _entity = _world.EntityManager.CreateEntity();
                _world.EntityManager.AddBuffer<TowerPlacementInputData>(_entity);
            }

            var collisionFilter = CollisionFilter.Default;
            collisionFilter.BelongsTo = belongsToTag.Value;
            collisionFilter.CollidesWith = collidesWithTag.Value;

            _world.EntityManager.GetBuffer<TowerPlacementInputData>(_entity).Add(new TowerPlacementInputData() {
                Value = new RaycastInput() {
                    Start = screenPointToRay.origin,
                    End = screenPointToRay.GetPoint(mainCamera.farClipPlane),
                    Filter = collisionFilter
                },
                TowerIndex = towerIndex
            });

            // if (Physics.Raycast(screenPointToRay, out var hit)) {
            //     var tower = hit.collider.GetComponent<Tower>();
            //     if (tower != null) {
            //         tower.OnClicked();
            //     }
            // }
        }

        private void OnDisable() {
            inputAction.Disable();
            inputAction.started -= OnMouseClicked;
            
            if (_world.IsCreated && _world.EntityManager.Exists(_entity)) {
                _world.EntityManager.DestroyEntity(_entity);
            }
        }
    }
}