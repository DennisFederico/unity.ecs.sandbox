using SystemLoader.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Random = Unity.Mathematics.Random;

namespace SystemLoader.Systems {
    
    [DisableAutoCreation]
    public partial struct SpawnBallSystem : ISystem {
        
        private float _accumulatedTime;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<BallSpawnerDataComponent>();
            state.RequireForUpdate<PrefabHoldingComponent>();
            state.RequireForUpdate<RandomSeeder>();
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var prefab = SystemAPI.GetSingleton<PrefabHoldingComponent>().Prefab;
            //Debug.Log($"On System Update: {SystemAPI.Time.ElapsedTime} | {SystemAPI.Time.DeltaTime}");
            var data = SystemAPI.GetComponentRW<BallSpawnerDataComponent>(state.SystemHandle);
            var random = SystemAPI.GetComponentRW<RandomSeeder>(state.SystemHandle);
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            if (data.ValueRO.SpawnedCount < data.ValueRO.SpawnCount) {
                _accumulatedTime += SystemAPI.Time.DeltaTime;
                var spawnCount = (int) (data.ValueRO.SpawnPerSecond * _accumulatedTime);
                if (spawnCount > 0) {
                    SpawnBalls(ecb, spawnCount, data.ValueRO.SpawnCenter, data.ValueRO.SpawnRange, prefab, ref random.ValueRW.Value, SystemAPI.Time.ElapsedTime);
                    _accumulatedTime = 0;
                }
                data.ValueRW.SpawnedCount += spawnCount;
                //Debug.Log($"Spawned {spawnCount} balls");
            }
        }
                
        [BurstCompile]
        private void SpawnBalls(EntityCommandBuffer ecb, int spawnCount, float3 valueROSpawnCenter, float3 valueROSpawnRange, Entity prefab, ref Random random, double time) {
            for (int i = 0; i < spawnCount; i++) {
                float3 offset = random.NextFloat3(-valueROSpawnRange, valueROSpawnRange);
                float3 position = valueROSpawnCenter + offset;
                var instance = ecb.Instantiate(prefab);
                ecb.SetComponent(instance, new LocalTransform() {
                    Position = position,
                    Rotation = quaternion.identity,
                    Scale = 1f
                });
                ecb.AddComponent(instance, new TimeToLiveComponent() {
                    TimeToLive = 7f,
                    BirthTime = time
                });
            }
        }


        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }
}