using Selection.Components;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Authoring;
using UnityEngine;

namespace Selection.MonoBehaviours {
    
    [AddComponentMenu("Unit Select/Deselect Manager")]
    public class UnitSelectManager : MonoBehaviour {
        [SerializeField] private Camera mainCamera;
        [SerializeField] private PhysicsCategoryTags belongsTo;
        [SerializeField] private PhysicsCategoryTags collidesWith;
        
        private World _world;
        //A Singleton that keeps the buffer of ray-casts
        private Entity _rayCastBuffer;
        
        private void OnEnable() {
            mainCamera = mainCamera == null ? Camera.main : mainCamera;
            _world = World.DefaultGameObjectInjectionWorld;
            
            if (_world.IsCreated && !_world.EntityManager.Exists(_rayCastBuffer)) {
                _rayCastBuffer = _world.EntityManager.CreateSingletonBuffer<RayCastBufferComponent>();
            }
        }

        private void Update() {
            if (Input.GetMouseButtonUp(0)) {
                var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                _world.EntityManager.GetBuffer<RayCastBufferComponent>(_rayCastBuffer).Add(new RayCastBufferComponent() {
                    Value = new RaycastInput() {
                        Start = ray.origin,
                        End = ray.GetPoint(mainCamera.farClipPlane),
                        Filter = new CollisionFilter() {
                            BelongsTo = belongsTo.Value,
                            CollidesWith = collidesWith.Value,
                        }
                    },
                    Additive = Input.GetKey(KeyCode.LeftShift)
                });
            }
        }

        private void OnDisable() {
            if (_world.IsCreated && _world.EntityManager.Exists(_rayCastBuffer)) {
                _world.EntityManager.DestroyEntity(_rayCastBuffer);
            }
        }
    }
}