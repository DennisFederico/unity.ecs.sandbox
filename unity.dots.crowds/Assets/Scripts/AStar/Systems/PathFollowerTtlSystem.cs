using AStar.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace AStar.Systems {

    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    [UpdateAfter(typeof(MovePathFollowerSystem))]
    [RequireMatchingQueriesForUpdate]
    public partial struct PathFollowerTtlSystem : ISystem {

        private EntityQuery _stoppedEntitiesWithTimeToLive;

        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            _stoppedEntitiesWithTimeToLive = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<PathFindingUserTag>()
                .WithDisabledRW<PathFollowIndex>()
                .WithAllRW<PathFollowerTimeToLive>()
                .Build(ref state);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            state.Dependency = new CleanTtlJob {
                Ecb = ecb.AsParallelWriter(),
                CurrentTime = SystemAPI.Time.ElapsedTime
            }.ScheduleParallel(_stoppedEntitiesWithTimeToLive, state.Dependency);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {
        }
    }

    [BurstCompile]
    public partial struct CleanTtlJob : IJobEntity {
        public EntityCommandBuffer.ParallelWriter Ecb;
        public double CurrentTime;

        private void Execute(in Entity entity, ref PathFollowerTimeToLive timeToLive) {
            if (timeToLive.StartTime == 0) {
                timeToLive.StartTime = CurrentTime;
                return;
            }

            if (timeToLive.StartTime + timeToLive.TimeToLive < CurrentTime) {
                Ecb.DestroyEntity(entity.Index, entity);
            }
        }
    }
}