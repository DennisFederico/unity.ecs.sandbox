using TowerDefense.Components;
using TowerDefense.MonoBehaviours;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

namespace TowerDefense.Systems {
    
    [DisableAutoCreation]
    public partial struct CreateVisualGameObjectSystem : ISystem {

        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<VisualGameObjectComponent>();
        }

        public void OnUpdate(ref SystemState state) {
            var ecbBos = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            foreach (var (visual, entity) in SystemAPI.Query<VisualGameObjectComponent>().WithEntityAccess()) {
                var go = Object.Instantiate(visual.VisualPrefab);
                go.AddComponent<EntityGameObjectDestroySync>().SetEntity(entity);
                ecbBos.AddComponent(entity, new VisualTransformComponent { Transform = go.transform });
                ecbBos.AddComponent(entity, new VisualAnimatorComponent { Animator = go.GetComponent<Animator>() });
                ecbBos.RemoveComponent<VisualGameObjectComponent>(entity);
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }
}