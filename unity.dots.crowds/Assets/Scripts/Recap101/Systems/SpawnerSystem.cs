using Recap101.Components;
using Unity.Burst;
using Unity.Entities;

namespace Recap101.Systems {
    [DisableAutoCreation]
    public partial struct SpawnerSystem : ISystem {
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<BeginFixedStepSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var ecb = SystemAPI.GetSingleton<BeginFixedStepSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            foreach (var (spawner, waypoints) in 
                     SystemAPI.Query<RefRW<SpawnerComponent>, DynamicBuffer<WaypointsComponent>>()) {
                spawner.ValueRW.SpawnTimer -= SystemAPI.Time.DeltaTime;
                if (spawner.ValueRO.SpawnTimer < 0) {
                    spawner.ValueRW.SpawnTimer = spawner.ValueRO.SpawnInterval;
                    var entity = ecb.Instantiate(spawner.ValueRO.Prefab);
                    var path = ecb.AddBuffer<WaypointsComponent>(entity);
                    path.AddRange(waypoints.AsNativeArray());
                    ecb.AddComponent<NextWaypointIndexComponent>(entity);
                }
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }
}