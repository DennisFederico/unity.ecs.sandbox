using TowerDefenseBase.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace TowerDefenseBase.Systems {

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public partial struct SpawnerSystem : ISystem {
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<SpawnerDataComponent>();
            state.RequireForUpdate<WaypointsAsset>();
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            foreach (var (spawner, waypoints) in 
                     SystemAPI.Query<RefRW<SpawnerDataComponent>, RefRO<WaypointsAsset>>()) {
                
                spawner.ValueRW.SpawnTimer -= SystemAPI.Time.DeltaTime;
                if (spawner.ValueRO.SpawnTimer > 0) continue;
                
                spawner.ValueRW.SpawnTimer = spawner.ValueRO.SpawnInterval;
                var entity = ecb.Instantiate(spawner.ValueRO.Prefab);

                ecb.AddComponent(entity, new WaypointsAsset { Waypoints = waypoints.ValueRO.Waypoints });
                ecb.SetComponent(entity, LocalTransform.FromPosition(waypoints.ValueRO.Waypoints.Value.Points[0]));
                ecb.AddComponent(entity, new NextWaypointIndexComponent() { Value = 1 });
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {
        }
    }
}