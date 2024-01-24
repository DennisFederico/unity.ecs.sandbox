using AStar.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace AStar.Systems {

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(PathfindingSystem))]
    public partial struct RemovePathFollowerSystem : ISystem {

        private EntityQuery _entityQuery;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            _entityQuery = SystemAPI.QueryBuilder().WithAll<PathFindingUserTag>().Build();
            state.RequireForUpdate(_entityQuery);
            state.RequireForUpdate<RemovePathFollowerRequest>();
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            //Check the amount of entities to remove
            var buffer = SystemAPI.GetSingletonBuffer<RemovePathFollowerRequest>();
            if (buffer.Length <= 0) return;
            int toRemove = 0;
            foreach (var removeRequest in buffer) {
                toRemove +=removeRequest.Value;
            }
            buffer.Clear();
            toRemove = math.min(_entityQuery.CalculateEntityCount(), toRemove);
            if (toRemove <= 0) return;

            //Grab an array of entities to remove
            var entityArray = _entityQuery.ToEntityArray(Allocator.Temp);
            ecb.DestroyEntity(entityArray.GetSubArray(0, toRemove));
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {
        }
    }
}