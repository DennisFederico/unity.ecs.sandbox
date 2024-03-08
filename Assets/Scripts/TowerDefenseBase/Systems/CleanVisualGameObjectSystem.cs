using TowerDefenseBase.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace TowerDefenseBase.Systems {
    
    /// <summary>
    /// This system is responsible for destroying the game object of the visual counterpart if the entity is deleted (missing the LocalTransform component). 
    /// </summary>
    public partial struct CleanVisualGameObjectSystem : ISystem {
        
        private EntityQuery _entitiesToClean;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            _entitiesToClean = SystemAPI.QueryBuilder()
                .WithAll<VisualTransformComponent>()
                .WithNone<LocalTransform>()
                .Build();
            //Update the system only if there are entities to clean
            state.RequireForUpdate(_entitiesToClean);
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }
        
        public void OnUpdate(ref SystemState state) {

            var ecbEos = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            foreach (var (vTransform, entity) in SystemAPI.Query<VisualTransformComponent>().WithNone<LocalTransform>().WithEntityAccess()) {
                if (vTransform.Transform != null) {
                    Object.Destroy(vTransform.Transform.gameObject);                    
                } 
                ecbEos.RemoveComponent<VisualTransformComponent>(entity);
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }
}