using Selection.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

namespace Selection.Systems {

    // [DisableAutoCreation]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(PhysicsSystemGroup))]
    [BurstCompile]
    public partial struct SelectMultipleUnitsSystem : ISystem {
        
        private EntityQuery _selectedUnits;

        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<SelectionColliderDataComponent>();
            state.RequireForUpdate<SimulationSingleton>();
            state.RequireForUpdate<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<PhysicsWorldSingleton>();
            state.RequireForUpdate<SelectedVisualPrefabComponent>();

            //NOT REQUIRED FOR UPDATE - ONLY THE SELECTION COLLIDER
            _selectedUnits = SystemAPI.QueryBuilder()
                .WithAll<SelectedUnitTag>()
                .Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            //TODO move to an ITriggerEventsJob
            //NOTE because we are in the main thread, probably not required when using ITriggerEventsJob
            //But it seems that there are more than one collider in the scene and we need to wait for all of them to be processed
            //It might be the Buffer used to creat and the system group in which we are updating
            state.CompleteDependency();

            var ecb = SystemAPI.GetSingleton<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            var simulationSingleton = SystemAPI.GetSingleton<SimulationSingleton>();
            var simulation = simulationSingleton.AsSimulation();
            
            //NOTE This could be a hack to early filter the collisions we are interested in, then we can "lookup" the component
            var pws = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

            var selectedPrefab = SystemAPI.GetSingleton<SelectedVisualPrefabComponent>();
            var selectionData = SystemAPI.GetSingleton<SelectionColliderDataComponent>();
            var compoundBelong = selectionData.BelongsTo.Value | selectionData.CollidesWith.Value;

            var newlySelectedUnits = new NativeHashSet<Entity>(16, Allocator.Temp);
            foreach (var triggerEvent in simulation.TriggerEvents) {
                // Debug.Log($"Trigger Event - Collision between {state.EntityManager.GetName(triggerEvent.EntityA)} and {state.EntityManager.GetName(triggerEvent.EntityB)}");
                //SHOULD USE A SINGLE LAYER CALLED SELECTABLE OR COMPONENT TAG... WHAT WOULD BE BETTER???
                var belongsA = pws.Bodies[triggerEvent.BodyIndexA].Collider.Value.GetCollisionFilter().BelongsTo;
                var belongsB = pws.Bodies[triggerEvent.BodyIndexB].Collider.Value.GetCollisionFilter().BelongsTo;
                if ((belongsA & belongsB) == 0 && (belongsA & compoundBelong) == 0 && (belongsB & compoundBelong) == 0) {
                    // Debug.Log("Unexpected collision - either both have the same filter or not in the expected filters");
                    continue;
                }
                // Select the unit that "collidesWidth"
                var entity = selectionData.CollidesWith.Value == belongsA ? triggerEvent.EntityA : triggerEvent.EntityB;
                newlySelectedUnits.Add(entity);
                //SelectUnit(ref state, ecb, entity, selectedPrefab.Value);
            }
            //Destroy the selection collider
            DestroySelectionCollider(ecb);

            //PROCESS SELECTION CASES
            var entityArray = _selectedUnits.ToEntityArray(Allocator.Temp);
            
            //NO UNITS SELECTED
            if (newlySelectedUnits.IsEmpty) {
                if (!selectionData.Additive && !_selectedUnits.IsEmpty) {
                    foreach (var currentlySelectedUnit in entityArray) {
                        DeselectUnit(ref state, ecb, currentlySelectedUnit);
                    }             
                }
                return;
            }
            
            //SOMETHING WAS SELECTED
            if (_selectedUnits.IsEmpty) {
                //Select all the units that are in the selection
                foreach (var entity in newlySelectedUnits) {
                    SelectUnit(ref state, ecb, entity, selectedPrefab.Value);
                }
                return;
            }
            
            //Filter the selection to only the newly selected units
            foreach (var currentlySelectedUnit in entityArray) {
                if (!newlySelectedUnits.Contains(currentlySelectedUnit)) {
                    newlySelectedUnits.Remove(currentlySelectedUnit);
                }
            }
            foreach (var entity in newlySelectedUnits) {
                SelectUnit(ref state, ecb, entity, selectedPrefab.Value);
            }
            
            // Deselect all the units that are not in the current selection
            if (!selectionData.Additive) {
                foreach (var currentlySelectedUnit in entityArray) {
                    if (!newlySelectedUnits.Contains(currentlySelectedUnit)) {
                        DeselectUnit(ref state, ecb, currentlySelectedUnit);
                    }
                }
            }
        }

        private void DestroySelectionCollider(EntityCommandBuffer ecb) {
            var selectionCollider = SystemAPI.GetSingletonEntity<SelectionColliderDataComponent>();
            ecb.DestroyEntity(selectionCollider);
        }

        private void SelectUnit(ref SystemState state, EntityCommandBuffer ecb, Entity entity, Entity selectedVisual) {
            if (SystemAPI.HasComponent<SelectedUnitTag>(entity)) return;
            //TODO ... Adding a components rearranges the memory, Use a enable/disable component tag instead
            ecb.AddComponent<SelectedUnitTag>(entity);
            // Add the Visual
            var ring = ecb.Instantiate(selectedVisual);
            ecb.AddComponent(ring, new Parent() {
                Value = entity
            });
        }
        
        private void DeselectUnit(ref SystemState state, EntityCommandBuffer ecb, Entity entity) {
            ecb.RemoveComponent<SelectedUnitTag>(entity);
            if (SystemAPI.HasBuffer<Child>(entity)) {
                var childBuffer = SystemAPI.GetBuffer<Child>(entity);
                foreach (var child in childBuffer) {
                    if (SystemAPI.HasComponent<DecalComponentTag>(child.Value)) {
                        ecb.DestroyEntity(child.Value);
                    }
                }
            }
        }
    }
}