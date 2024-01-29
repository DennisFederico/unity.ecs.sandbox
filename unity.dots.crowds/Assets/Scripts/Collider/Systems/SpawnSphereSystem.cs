using Collider.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Collider.Systems {
    
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    [BurstCompile]
    public partial struct SpawnSphereSystem : ISystem {
        
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<SpawnRequestComponentBuffer>();
            state.RequireForUpdate<SpheresHolderComponent>();
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var spawnRequestBuffer = SystemAPI.GetSingletonBuffer<SpawnRequestComponentBuffer>();
            if (spawnRequestBuffer.Length == 0) return;
            
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            var prefabHolder = SystemAPI.GetSingleton<SpheresHolderComponent>();
            
            foreach (var spawnRequest in spawnRequestBuffer) {
                float3 point = spawnRequest.Ray.GetPoint(spawnRequest.Distance);
                var sphere = ecb.Instantiate(prefabHolder.GetPrefabForClick(spawnRequest.MouseButton));
                ecb.AddComponent(sphere, LocalTransform.FromPosition(point));
                ecb.AddComponent(sphere, new TimeToLiveComponent() {
                    TimeToLive = 3f,
                    CreatedAt = SystemAPI.Time.ElapsedTime
                });
            }
            spawnRequestBuffer.Clear();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {

        }
    }
}