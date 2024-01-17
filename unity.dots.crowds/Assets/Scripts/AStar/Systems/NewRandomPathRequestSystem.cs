using AStar.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Utils.Narkdagas.Ecs;
using Utils.Narkdagas.PathFinding;

namespace AStar.Systems {

    [DisableAutoCreation]
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(PathfindingSystem))]
    [RequireMatchingQueriesForUpdate]
    public partial struct NewRandomPathRequestSystem : ISystem {

        private EntityQuery _entitiesWithNoPathAndNoTimeToLive;

        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<RandomSeeder>();
            state.RequireForUpdate<GridSingletonComponent>();

            _entitiesWithNoPathAndNoTimeToLive = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<PathFindingUserTag>()
                .WithDisabledRW<PathFollowIndex>()
                .WithDisabledRW<PathFindingRequest>()
                .WithAll<LocalTransform>()
                .WithNone<PathFollowerTimeToLive>()
                .Build(ref state);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var ecs = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            var random = SystemAPI.GetSingletonRW<RandomSeeder>();
            var gridInfo = SystemAPI.GetSingleton<GridSingletonComponent>();
            var grid = SystemAPI.GetBuffer<PathNode>(SystemAPI.GetSingletonEntity<GridSingletonComponent>());

            state.Dependency = new NewRandomPathRequestJob() {
                Ecb = ecs.AsParallelWriter(),
                Random = random,
                GridInfo = gridInfo,
                Grid = grid
            }.ScheduleParallel(_entitiesWithNoPathAndNoTimeToLive, state.Dependency);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {
        }

        [BurstCompile]
        public partial struct NewRandomPathRequestJob : IJobEntity {

            public EntityCommandBuffer.ParallelWriter Ecb;

            [NativeDisableUnsafePtrRestriction]
            public RefRW<RandomSeeder> Random;
            [ReadOnly] public GridSingletonComponent GridInfo;
            [ReadOnly] public DynamicBuffer<PathNode> Grid;

            private void Execute(in Entity entity, ref PathFindingRequest pathFindingRequest, in LocalTransform transform) {
                var targetXY = GetRandomWalkablePosition();
                var targetPosition = GridInfo.GetWorldPosition(targetXY);
                var startPosition = transform.Position;

                pathFindingRequest.StartPosition = startPosition;
                pathFindingRequest.EndPosition = targetPosition;
                Ecb.SetComponentEnabled<PathFindingRequest>(entity.Index, entity, true);
            }

            private int2 GetRandomWalkablePosition() {
                var randomXY = Random.ValueRW.NextSeed.NextInt2(int2.zero, new int2(GridInfo.Width, GridInfo.Height));
                if (Grid[GridInfo.GetIndex(randomXY)].IsWalkable) {
                    return randomXY;
                }

                return GetRandomWalkablePosition();
            }
        }
    }
}