using SimpleCrowdsSpawn.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;

namespace SimpleCrowdsSpawn.Systems {

    [UpdateBefore(typeof(PlaceSpawnerSystem))]
    public partial struct SpawnSystem : ISystem {

        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<CrowdSpawner>();
            state.RequireForUpdate<SpawnRequestBuffer>();
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var spawnRequestBuffer = SystemAPI.GetSingletonBuffer<SpawnRequestBuffer>();
            if (spawnRequestBuffer.Length == 0) return;
            var crowdSpawner = SystemAPI.GetSingleton<CrowdSpawner>();
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            var spawnerTransform = SystemAPI.GetComponentRO<LocalTransform>(crowdSpawner.Spawner).ValueRO;

            foreach (var spawnRequest in spawnRequestBuffer) {
                NativeArray<Entity> entities = new NativeArray<Entity>(spawnRequest.Amount, Allocator.Temp);
                ecb.Instantiate(crowdSpawner.Prefab, entities);
                var localTransform = LocalTransform.FromPosition(spawnerTransform.Position);
                ecb.AddComponent(entities, localTransform);
                ecb.AddComponent(entities, new TargetPosition { Value = spawnerTransform.Position });
            }
            spawnRequestBuffer.Clear();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {
        }
    }
}