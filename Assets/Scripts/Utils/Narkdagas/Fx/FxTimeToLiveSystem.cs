using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Utils.Narkdagas.Fx {
    
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public partial struct FxTimeToLiveSystem : ISystem {
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var endSimulationBuffer = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            var asParallelWriter = endSimulationBuffer.AsParallelWriter();
            new FxTimeToLiveEntityJob() {
                EntityBuffer = asParallelWriter,
                DeltaTime = SystemAPI.Time.DeltaTime
            }.ScheduleParallel();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        private partial struct FxTimeToLiveEntityJob : IJobEntity {

            public EntityCommandBuffer.ParallelWriter EntityBuffer;
            [ReadOnly] public float DeltaTime;
            
            [BurstCompile]
            private void Execute(ref FxTimeToLiveComponent ttl, Entity entity, [ChunkIndexInQuery] int ci, [EntityIndexInChunk] int ei) {
                ttl.Value -= DeltaTime;
                if (ttl.Value < 0) {
                    EntityBuffer.DestroyEntity(ci+ei, entity);
                }
            }
        }
    }
}