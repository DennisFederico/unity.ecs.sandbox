using Collider.Components;
using Unity.Burst;
using Unity.Entities;

namespace Collider.Systems {
    
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public partial struct TimeToLiveSystem : ISystem {
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<TimeToLiveComponent>();
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            foreach (var (ttl, entity) in SystemAPI.Query<RefRO<TimeToLiveComponent>>().WithEntityAccess()) {
                if (ttl.ValueRO.CreatedAt + ttl.ValueRO.TimeToLive < SystemAPI.Time.ElapsedTime) {
                    ecb.DestroyEntity(entity);
                }
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {

        }
    }
}