using Selection.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

namespace Selection.Systems {
    
    // [DisableAutoCreation]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(PhysicsSystemGroup))]
    [BurstCompile]
    public partial struct SelectMultipleUnitsSystem : ISystem {
        //TODO - Evaluate or measure the performance against using Enable/disable tags instead of adding/removing components
        //TODO - Also we could have a single query over those components to work trigger results
        
        //private ComponentLookup<SelectedUnitTag> _positionLookup;
        // private ComponentLookup<HealthComponent> _enemyHealthLookup;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<SelectionColliderDataComponent>();
            state.RequireForUpdate<SimulationSingleton>();
            state.RequireForUpdate<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<PhysicsWorldSingleton>();
            state.RequireForUpdate<SelectedVisualPrefabComponent>();
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            // Debug.Log("System - SelectMultipleUnitsSystem - Update");
            state.CompleteDependency(); //NOTE because we are in the main thread, probably not required when using ITriggerEventsJob
            
            var ecb = SystemAPI.GetSingleton<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            var simulationSingleton = SystemAPI.GetSingleton<SimulationSingleton>();
            var simulation = simulationSingleton.AsSimulation();
            
            //NOTE This could be a hack to early filter the collisions we are interested in, then we can "lookup" the component
            var pws = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            
            var selectedPrefab = SystemAPI.GetSingleton<SelectedVisualPrefabComponent>();
            var selectionData = SystemAPI.GetSingleton<SelectionColliderDataComponent>();
            var compoundBelong = selectionData.BelongsTo.Value | selectionData.CollidesWith.Value;
            
            //NOTE Triggers are "streamed", thus we don't know the number beforehand, perfect for jobs in parallel
            foreach (var triggerEvent in simulation.TriggerEvents) {
                // Debug.Log($"Trigger Event - Collision between {state.EntityManager.GetName(triggerEvent.EntityA)} and {state.EntityManager.GetName(triggerEvent.EntityB)}");
                //SHOULD USE A SINGLE LAYER CALLED SELECTABLE OR COMPONENT TAG... WHAT WOULD BE BETTER???
                var belongsA = pws.Bodies[triggerEvent.BodyIndexA].Collider.Value.GetCollisionFilter().BelongsTo;
                var belongsB = pws.Bodies[triggerEvent.BodyIndexB].Collider.Value.GetCollisionFilter().BelongsTo;
                if ((belongsA & belongsB) == 0 && (belongsA & compoundBelong) == 0 && (belongsB & compoundBelong) == 0) {
                    Debug.Log("Unexpected collision - either both have the same filter or not in the expected filters");
                    continue;
                }

                // Select the unit that "collidesWidth"
                var entity = selectionData.CollidesWith.Value == belongsA ? triggerEvent.EntityA : triggerEvent.EntityB;
                SelectUnit(ref state, ecb, entity, selectedPrefab.Value);
                
                // WE COULD USE LOOKUPS TO CHECK FOR TAG COMPONENTS BUT NOT SURE WHAT IS THE MOST PERFORMANT APPROACH ONLY KNOW THAT
                // YOU ARE FORCED TO CREATE A TAG COMPONENT WHICH ON THE OTHER HAND MIGHT BE THE MOST CONVENIENT APPROACH ALIGNED WITH THE ECS SPIRIT
                // //var type1Lookup = SystemAPI.GetComponentLookup<Type1>(true);
                // //var type1Lookup = SystemAPI.GetComponentLookup<Type2>(true);
                // if (ProjectileImpactLookup.TryGetComponent(triggerEvent.EntityA, out var projectileVfx) && HealthLookup.TryGetComponent(triggerEvent.EntityB, out enemyHealth)) {
                //     projectileEntity = triggerEvent.EntityA;
                //     enemyEntity = triggerEvent.EntityB;
                // } else if (ProjectileImpactLookup.TryGetComponent(triggerEvent.EntityB, out projectileVfx) && HealthLookup.TryGetComponent(triggerEvent.EntityA, out enemyHealth)) {
                //     projectileEntity = triggerEvent.EntityB;
                //     enemyEntity = triggerEvent.EntityA;
            }
            
            //TODO handle Additivity (if not additive, deselect all units)
            //This may need a couple of queries to "selected" tag <-- Could be a enabled/disabled component tag
            
            //Destroy the selection collider
            DestroySelectionCollider(ecb);
        }

        private void DestroySelectionCollider(EntityCommandBuffer ecb) {
            var selectionCollider = SystemAPI.GetSingletonEntity<SelectionColliderDataComponent>();
            ecb.DestroyEntity(selectionCollider);
            // Debug.Log("System - SelectMultipleUnitsSystem - Should Stop");
        }
        
        private void SelectUnit(ref SystemState state, EntityCommandBuffer ecb, Entity entity, Entity selectedVisual) {
            if (SystemAPI.HasComponent<SelectedUnitTag>(entity)) return;
            //TODO BOLLOCKS ... Adding a components rearranges the memory, What if we use a enable/disable component?
            ecb.AddComponent<SelectedUnitTag>(entity);
            // Add the Visual
            var ring = ecb.Instantiate(selectedVisual);
            ecb.AddComponent(ring, new Parent() {
                Value = entity
            });
        }
    }
}