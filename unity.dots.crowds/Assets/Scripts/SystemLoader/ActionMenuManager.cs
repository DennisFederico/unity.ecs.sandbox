using CodeMonkey.Utils;
using SystemLoader.Components;
using SystemLoader.Systems;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Utils.Narkdagas.SceneManagement;
using Random = UnityEngine.Random;
using SpawnBallSystem = SystemLoader.Systems.SpawnBallSystem;

namespace SystemLoader {
    public class ActionMenuManager : MonoBehaviour {
        [SerializeField] private Button_UI startSystemsButton;
        [SerializeField] private Button_UI stopSystemsButton;
        [SerializeField] private Button_UI mainMenuButton;
        private World _world;
        private bool _started;

        private void OnEnable() {
            _world = World.DefaultGameObjectInjectionWorld;
            startSystemsButton.ClickFunc = StartSystems;
            stopSystemsButton.ClickFunc = StopSystems;
            mainMenuButton.ClickFunc = () => {
                Debug.Log("Loading MainMenuScene");
                SceneLoader.Load(SceneLoader.Scenes.MainMenuScene);
            };
        }

        private void StartSystems() {
            if (!_started && _world.IsCreated) {
                Debug.Log("Starting Systems");
                var simulationSystemGroup = _world.GetExistingSystemManaged<SimulationSystemGroup>();
                var spawnBallSystemHandle = _world.CreateSystem<SpawnBallSystem>();
                simulationSystemGroup.AddSystemToUpdateList(spawnBallSystemHandle);
                _world.EntityManager.AddComponent<BallSpawnerDataComponent>(spawnBallSystemHandle);
                _world.EntityManager.SetComponentData(spawnBallSystemHandle, new BallSpawnerDataComponent {
                    SpawnCenter = new float3(0, 4, 0),
                    SpawnRange = new float3(3, 2, 1),
                    SpawnCount = 1000,
                    SpawnPerSecond = 10,
                    SpawnedCount = 0
                });
                _world.EntityManager.AddComponent<RandomSeeder>(spawnBallSystemHandle);
                _world.EntityManager.SetComponentData(spawnBallSystemHandle, new RandomSeeder {
                    Value = new Unity.Mathematics.Random((uint) Random.Range(1, uint.MaxValue))
                });

                //CREATE TTL SYSTEM Only if it does not exist
                if (_world.GetExistingSystem<TimeToLiveSystem>() == SystemHandle.Null) {
                    simulationSystemGroup.AddSystemToUpdateList(_world.CreateSystem<TimeToLiveSystem>());                    
                }
            }
            _started = true;
        }
        
        private void StopSystems() {
            
            if (_started && _world.IsCreated) {
                Debug.Log("Stopping Systems");
                var simulationSystemGroup = _world.GetExistingSystemManaged<SimulationSystemGroup>();
                var spawnBallSystemHandle = _world.GetExistingSystem<SpawnBallSystem>();
                simulationSystemGroup.RemoveSystemFromUpdateList(spawnBallSystemHandle);
                _world.DestroySystem(spawnBallSystemHandle);
                //TTL System destroys all TTL entities when destroyed
                var ttlSystem = _world.GetExistingSystem<TimeToLiveSystem>();
                simulationSystemGroup.RemoveSystemFromUpdateList(ttlSystem);
                _world.DestroySystem(ttlSystem);
            }
            _started = false;
        }

        private void OnDisable() {
            startSystemsButton.ClickFunc -= StartSystems;
            stopSystemsButton.ClickFunc -= StopSystems;
            mainMenuButton.ClickFunc = null;
        }
    }
}