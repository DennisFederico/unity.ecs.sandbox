using AStar.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace AStar.Systems {

    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateBefore(typeof(PathfindingSystem))]
    public partial struct CreatePathFollowerSystem : ISystem {

        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<PrefabHoldingSingleton>();
            state.RequireForUpdate<CreateNewPathFollowerRequest>();
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            var buffer = SystemAPI.GetSingletonBuffer<CreateNewPathFollowerRequest>();
            if (buffer.Length <= 0) return;

            var prefab = SystemAPI.GetSingleton<PrefabHoldingSingleton>().Prefab;

            foreach (var createRequest in buffer) {
                var instance = ecb.Instantiate(prefab);
                ecb.SetComponent(instance, new PathFindingRequest {
                    StartPosition = createRequest.StartPosition,
                    EndPosition = createRequest.EndPosition
                });
                ecb.SetComponentEnabled<PathFindingRequest>(instance, true);
                ecb.SetComponent(instance, new LocalTransform() {
                    Position = createRequest.StartPosition,
                    Rotation = quaternion.identity,
                    Scale = 1f
                });
                if (createRequest.TimeToLive > 0f) {
                    ecb.AddComponent(instance, new PathFollowerTimeToLive {
                        StartTime = 0,
                        TimeToLive = createRequest.TimeToLive
                    });
                }
            }

            buffer.Clear();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {
        }
    }
}