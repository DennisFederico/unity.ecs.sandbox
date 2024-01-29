using Collider.Components;
using Unity.Entities;
using UnityEngine;

namespace Collider {
    public class ColliderTestInputManager : MonoBehaviour {

        [SerializeField] private Camera mainCamera;

        private World _world;
        private Entity _spawnRequestBuffer;

        private void OnEnable() {
            mainCamera = mainCamera == null ? Camera.main : mainCamera;
            _world = World.DefaultGameObjectInjectionWorld;

            if (_world.IsCreated) {
                if (!_world.EntityManager.Exists(_spawnRequestBuffer)) {
                    _spawnRequestBuffer = _world.EntityManager.CreateSingletonBuffer<SpawnRequestComponentBuffer>();
                }
            }
        }

        private void OnDisable() {
            if (_world.IsCreated) {
                if (_world.EntityManager.Exists(_spawnRequestBuffer)) {
                    _world.EntityManager.DestroyEntity(_spawnRequestBuffer);
                }
            }
        }

        private void Update() {
            int clickedButton = -1;
            if (Input.GetMouseButtonDown(0)) {
                clickedButton = 0;
            }

            if (Input.GetMouseButtonDown(1)) {
                clickedButton = 1;
            }

            if (Input.GetMouseButtonDown(2)) {
                clickedButton = 2;
            }

            if (clickedButton != -1) {
                var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                _world.EntityManager.GetBuffer<SpawnRequestComponentBuffer>(_spawnRequestBuffer)
                    .Add(new SpawnRequestComponentBuffer() {
                        MouseButton = clickedButton,
                        Ray = ray,
                        Distance = 15f
                    });
            }
        }
    }
}