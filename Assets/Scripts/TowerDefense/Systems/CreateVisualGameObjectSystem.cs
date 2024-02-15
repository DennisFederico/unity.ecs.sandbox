using TowerDefense.Components;
using TowerDefense.MonoBehaviours;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace TowerDefense.Systems {
    
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(SpawnerSystem))]
    public partial struct CreateVisualGameObjectSystem : ISystem {

        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<VisualGameObjectComponent>();
        }

        public void OnUpdate(ref SystemState state) {
            var ecbBos = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            foreach (var (visual, transform, entity) in SystemAPI.Query<VisualGameObjectComponent, LocalTransform>().WithEntityAccess()) {
                var go = Object.Instantiate(visual.VisualPrefab);
                go.transform.position = transform.Position;
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