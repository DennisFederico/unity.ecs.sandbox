using TowerDefense.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace TowerDefense.Systems {
    
    public partial struct CleanVisualGameObjectSystem : ISystem {
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }
        
        public void OnUpdate(ref SystemState state) {

            var ecbEos = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);
            
            foreach (var (vTransform, entity) in SystemAPI.Query<VisualTransformComponent>().WithNone<LocalTransform>().WithEntityAccess()) {
                Object.Destroy(vTransform.Transform.gameObject);
                ecbEos.RemoveComponent<VisualTransformComponent>(entity);
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }
}