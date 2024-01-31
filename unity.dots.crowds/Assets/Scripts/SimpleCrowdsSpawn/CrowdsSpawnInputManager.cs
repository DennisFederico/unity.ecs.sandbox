using SimpleCrowdsSpawn.Components;
using Unity.Entities;
using UnityEngine;

namespace SimpleCrowdsSpawn {
    public class CrowdsSpawnInputManager : MonoBehaviour {

        [SerializeField] private Camera mainCamera;
        [SerializeField] private LayerMask floorLayerMask;

        private World _world;
        private Entity _setSpawnerRequestBuffer;
        private Entity _spawnRequestBuffer;

        private void OnEnable() {
            mainCamera = mainCamera == null ? Camera.main : mainCamera;
            _world = World.DefaultGameObjectInjectionWorld;
            if (_world.IsCreated) {
                if (!_world.EntityManager.Exists(_setSpawnerRequestBuffer)) {
                    _setSpawnerRequestBuffer = _world.EntityManager.CreateSingletonBuffer<PlaceSpawnerRequestBuffer>();
                }
                if (!_world.EntityManager.Exists(_spawnRequestBuffer)) {
                    _spawnRequestBuffer = _world.EntityManager.CreateSingletonBuffer<SpawnRequestBuffer>();
                }
            }
        }

        private void OnDisable() {
            if (_world.IsCreated) {
                if (_world.EntityManager.Exists(_setSpawnerRequestBuffer)) {
                    _world.EntityManager.DestroyEntity(_setSpawnerRequestBuffer);
                }
                if (_world.EntityManager.Exists(_spawnRequestBuffer)) {
                    _world.EntityManager.DestroyEntity(_spawnRequestBuffer);
                }
            }
        }

        private void Update() {
            if (Input.GetMouseButtonDown(0)) {
                var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out var hit, 100f, floorLayerMask)) {
                    var rotation = Quaternion.LookRotation(new Vector3(ray.direction.x, 0, ray.direction.z), Vector3.up);
                    _world.EntityManager.GetBuffer<PlaceSpawnerRequestBuffer>(_setSpawnerRequestBuffer)
                        .Add(new PlaceSpawnerRequestBuffer() {
                            SelectRandom = false,
                            Position = hit.point,
                            Rotation = rotation
                        });
                }
            }

            if (Input.GetKeyDown(KeyCode.Space)) {
                _world.EntityManager.GetBuffer<PlaceSpawnerRequestBuffer>(_setSpawnerRequestBuffer)
                    .Add(new PlaceSpawnerRequestBuffer() {
                        SelectRandom = true
                    });
            }

            if (Input.GetKeyDown(KeyCode.KeypadPlus)) {
                _world.EntityManager.GetBuffer<SpawnRequestBuffer>(_spawnRequestBuffer)
                    .Add(new SpawnRequestBuffer() {
                        Amount = 10
                    });
            }
        }
    }
}