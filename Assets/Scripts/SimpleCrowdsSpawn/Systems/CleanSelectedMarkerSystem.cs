using SimpleCrowdsSpawn.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace SimpleCrowdsSpawn.Systems {
    public partial struct CleanSelectedMarkerSystem : ISystem {
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<CrowdSpawner>();
            state.RequireForUpdate<SelectedMarker>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            var spawnerReference = SystemAPI.GetSingletonRW<CrowdSpawner>();
            foreach (var (_, entity) in SystemAPI.Query<SelectedMarker>().WithNone<LocalTransform>().WithEntityAccess()) {
                spawnerReference.ValueRW.Spawner = Entity.Null;
                ecb.RemoveComponent<SelectedMarker>(entity);
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {

        }
    }
}