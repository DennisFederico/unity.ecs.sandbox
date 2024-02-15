using TowerDefense.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace TowerDefense.Systems {
    
    [DisableAutoCreation]
    public partial struct TimeToLiveSystem : ISystem {
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var endSimulationBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            var asParallelWriter = endSimulationBuffer.AsParallelWriter();
            new TimeToLiveEntityJob() {
                EntityBuffer = asParallelWriter,
                DeltaTime = SystemAPI.Time.DeltaTime
            }.ScheduleParallel();

            // foreach (var (ttl, entity) in SystemAPI.Query<RefRW<TimeToLiveComponent>>().WithEntityAccess()) {
            //     ttl.ValueRW.Value -= SystemAPI.Time.DeltaTime;
            //     if (ttl.ValueRO.Value < 0) {
            //         ecbBos.DestroyEntity(entity);
            //     }
            // }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        private partial struct TimeToLiveEntityJob : IJobEntity {

            public EntityCommandBuffer.ParallelWriter EntityBuffer;
            [ReadOnly] public float DeltaTime;
            
            [BurstCompile]
            private void Execute(ref TimeToLiveComponent ttl, Entity entity, [ChunkIndexInQuery] int ci, [EntityIndexInChunk] int ei) {
                ttl.Value -= DeltaTime;
                if (ttl.Value < 0) {
                    EntityBuffer.DestroyEntity(ci+ei, entity);
                }
            }
        }
    }
}