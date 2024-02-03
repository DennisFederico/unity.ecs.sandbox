using Recap101.Aspects;
using Unity.Burst;
using Unity.Entities;

namespace Recap101.Systems {

    public partial struct MoveByAspectSystem : ISystem {
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            
            var ecbBos = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);
            
            foreach (var (moveByAspect, entity) in SystemAPI.Query<PathFollowerAspect>().WithEntityAccess()) {
                moveByAspect.FollowPath(SystemAPI.Time.DeltaTime);
                if (moveByAspect.IsAtEndOfPath()) {
                    ecbBos.DestroyEntity(entity);
                }
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }
}