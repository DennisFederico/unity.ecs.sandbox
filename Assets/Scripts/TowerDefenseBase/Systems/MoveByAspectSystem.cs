using TowerDefenseBase.Aspects;
using TowerDefenseBase.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace TowerDefenseBase.Systems {
    
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(TransformSystemGroup))]
    public partial struct MoveByAspectSystem : ISystem {
        
        private EntityQuery _movableEntities;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            _movableEntities = SystemAPI.QueryBuilder()
                .WithAll<MoveSpeedComponent>()
                .WithAll<NextWaypointIndexComponent>()
                .WithAll<LocalTransform>()
                .Build();
            //Only update if there are entities to move
            state.RequireForUpdate(_movableEntities);
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            
            var ecbBos = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);
            
            foreach (var (moveByAspect, entity) in SystemAPI.Query<PathFollowingAspect>().WithEntityAccess()) {
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