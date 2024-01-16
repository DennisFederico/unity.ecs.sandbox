using AStar.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Utils.Narkdagas.PathFinding;

namespace AStar.Systems {

    [BurstCompile]
    public partial struct PathfindingSystem : ISystem {

        private EntityQuery _entitiesWithPathRequest;

        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<GridSingletonComponent>();
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            
            
            _entitiesWithPathRequest = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<PathFindingUserTag>()
                .WithAll<PathFindingRequest>()
                .WithAllRW<PathFollowIndex>()
                .WithAllRW<PathPositionElement>()
                .Build(ref state);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var gridInfo = SystemAPI.GetSingleton<GridSingletonComponent>();
            var grid = SystemAPI.GetSingletonBuffer<PathNode>(true);
            
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            //_pathBufferLookup.Update(ref state);
            state.Dependency = new FindPathForEntityJob {
                GridInfo = gridInfo,
                Grid = grid.AsNativeArray(),
                Ecb = ecb,
                //PathBufferLookup = _pathBufferLookup
            }.Schedule(_entitiesWithPathRequest, state.Dependency);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {
        }
    }
}