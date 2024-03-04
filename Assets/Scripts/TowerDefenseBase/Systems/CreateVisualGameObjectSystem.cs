using TowerDefenseBase.Components;
using TowerDefenseBase.Mono;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace TowerDefenseBase.Systems {
    
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(SpawnerSystem))]
    public partial struct CreateVisualGameObjectSystem : ISystem {

        private EntityQuery _entitiesToCreateVisual;
        
        public void OnCreate(ref SystemState state) {
            _entitiesToCreateVisual = SystemAPI.QueryBuilder()
                .WithAll<VisualGameObjectComponent>()
                .WithNone<VisualTransformComponent>()
                .WithNone<VisualAnimatorComponent>()
                .Build();
            //Update the system only if there are entities to work with
            state.RequireForUpdate(_entitiesToCreateVisual);
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
        }

        public void OnUpdate(ref SystemState state) {
            var ecbBos = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);

            //Recently "spawned" entities will have a VisualGameObjectComponent, but no VisualTransformComponent
            foreach (var (visual, transform, entity) in SystemAPI.Query<VisualGameObjectComponent, LocalTransform>().WithEntityAccess()) {
                //We create the visual representation of the entity in the mono world from the provided prefab
                //update its position and rotation to match the entity's transform and then remove the VisualGameObjectComponent
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