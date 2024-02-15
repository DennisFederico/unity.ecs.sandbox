using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using PathFollowerAspect = TowerDefense.Aspects.PathFollowerAspect;

namespace TowerDefense.Systems {
    
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(TransformSystemGroup))]
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