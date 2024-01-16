using AStar.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace AStar.Systems {
    
    [RequireMatchingQueriesForUpdate]
    [BurstCompile]
    public partial struct MovePathFollowerSystem : ISystem {
        
        private EntityQuery _entitiesFollowingAPath;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            _entitiesFollowingAPath = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<PathFindingUserTag>()
                .WithAllRW<PathFollowIndex>()
                .WithAll<PathPositionElement>()
                .WithAllRW<LocalTransform>()
                .WithAll<MoveSpeed>()
                .WithDisabled<PathFindingRequest>()
                .Build(ref state);
            
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            
            state.Dependency = new MovePathFollowerJob() {
                Ecb = ecb,
                DeltaTime = SystemAPI.Time.DeltaTime
            }.Schedule(_entitiesFollowingAPath, state.Dependency);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {
        }
    }

    [BurstCompile]
    public partial struct MovePathFollowerJob : IJobEntity {
        
        public EntityCommandBuffer Ecb;
        public float DeltaTime;
        
        //TODO PATH REQUEST TO RANDOM POINT
        private void Execute(in Entity entity, ref PathFollowIndex pathFollowIndex, DynamicBuffer<PathPositionElement> currentPath, ref LocalTransform transform, MoveSpeed speed) {
            
            float3 direction = currentPath[pathFollowIndex.Value].Position - transform.Position;
            
            if (math.length(direction) < 0.15f) {
                pathFollowIndex.Value = pathFollowIndex.Value - 1;
                
                if (pathFollowIndex.Value < 0) {
                    Ecb.SetComponentEnabled<PathFollowIndex>(entity, false);
                    return;
                }
                direction = currentPath[pathFollowIndex.Value].Position - transform.Position;
            }
            
            transform.Position += math.normalize(direction) * (speed.Value * DeltaTime);
        }
    }
}