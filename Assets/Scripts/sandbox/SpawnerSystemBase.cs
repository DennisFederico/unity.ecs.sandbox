using Unity.Entities;
using UnityEngine;

namespace sandbox {
    public partial class SpawnerSystemBase : SystemBase {
        protected override void OnUpdate() {
            //Check the amount spawned so far
            var entityQuery = EntityManager.CreateEntityQuery(typeof(PlayerTag));
            var calculateEntityCount = entityQuery.CalculateEntityCount();
            int spawnAmount = 50;
            if (calculateEntityCount >= spawnAmount) return;
            
            var spawnerComponent = SystemAPI.GetSingleton<SpawnerComponent>();
            var randomComponent = SystemAPI.GetSingletonRW<RandomComponent>();

            //Spawn using a buffe r / Sync using the buffer
            var entityCommandBuffer = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
            var entity = entityCommandBuffer.Instantiate(spawnerComponent.Prefab);
            entityCommandBuffer.SetComponent(entity, new Speed {
                Value = randomComponent.ValueRW.Random.NextFloat(2f, 8f)
            });

            //Spawn a new entity
            //EntityManager.Instantiate(spawnerComponent.Prefab);
        }
    }
}