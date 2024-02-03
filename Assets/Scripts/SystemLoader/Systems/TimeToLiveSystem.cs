using SystemLoader.Components;
using Unity.Burst;
using Unity.Entities;

namespace SystemLoader.Systems {
    
    [DisableAutoCreation]
    public partial struct TimeToLiveSystem : ISystem {
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<TimeToLiveComponent>();
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            var elapsedTime = SystemAPI.Time.ElapsedTime;
            foreach (var (ttl, entity) in SystemAPI.Query<RefRO<TimeToLiveComponent>>().WithEntityAccess()) {
                if (elapsedTime - ttl.ValueRO.BirthTime > ttl.ValueRO.TimeToLive) {
                    ecb.DestroyEntity(entity);
                }
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {
            //Destroy all TTL entities
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            foreach (var (_, entity) in SystemAPI.Query<RefRO<TimeToLiveComponent>>().WithEntityAccess()) {
                ecb.DestroyEntity(entity);
            }
        }
    }
}