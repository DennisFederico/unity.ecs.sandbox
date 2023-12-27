using TowerDefense.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace TowerDefense.Systems {
    public partial struct SpawnerSystem : ISystem {
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<BeginFixedStepSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var ecb = SystemAPI.GetSingleton<BeginFixedStepSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            foreach (var (spawner, spawnPos, waypoints) in 
                     SystemAPI.Query<RefRW<SpawnerDataComponent>, RefRO<LocalToWorld>, RefRO<BlobPathAsset>>()) {
                spawner.ValueRW.SpawnTimer -= SystemAPI.Time.DeltaTime;
                if (spawner.ValueRO.SpawnTimer < 0) {
                    spawner.ValueRW.SpawnTimer = spawner.ValueRO.SpawnInterval;
                    var entity = ecb.Instantiate(spawner.ValueRO.Prefab);
                    
                    // CHANGED DynamicBuffer<WaypointsComponent>
                    // var path = ecb.AddBuffer<WaypointsComponent>(entity);
                    // path.AddRange(waypoints.AsNativeArray());
                    
                    ecb.AddComponent<BlobPathAsset>(entity, new BlobPathAsset {
                        Path = waypoints.ValueRO.Path
                    });
                    ecb.AddComponent<NextWaypointIndexComponent>(entity);
                    ecb.SetComponent(entity, new LocalTransform {
                        Position = spawnPos.ValueRO.Position,
                        Rotation = spawnPos.ValueRO.Rotation,
                        Scale = 1
                    });
                }
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }
}