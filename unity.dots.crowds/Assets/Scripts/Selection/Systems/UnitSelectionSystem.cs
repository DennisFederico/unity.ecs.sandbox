using Selection.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;

namespace Selection.Systems {
    
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial struct UnitSelectionSystem : ISystem {
        
        //IMPORTANT: RayCasts are sent from the MonoBehavior world, since using a Camera in the ECS world is not Burst compatible
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<RayCastBufferComponent>();
            state.RequireForUpdate<PhysicsWorldSingleton>();
            state.RequireForUpdate<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<SelectedVisualPrefabComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var pws = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            var physicsWorld = pws.PhysicsWorld;
            var selectedPrefab = SystemAPI.GetSingleton<SelectedVisualPrefabComponent>();
            var ecb = SystemAPI.GetSingleton<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            var rayCastBuffer = SystemAPI.GetSingletonBuffer<RayCastBufferComponent>();
            foreach (var rayCastComponent in rayCastBuffer) {
                if (physicsWorld.CastRay(rayCastComponent.Value, out var hit)) {
                    // Debug.Log($"Hit {hit.Position} - Entity? {state.EntityManager.GetName(hit.Entity)}");
                    if (!SystemAPI.HasComponent<SelectedUnitTag>(hit.Entity)) {
                        // Debug.Log($"Adding SelectedUnitTag to {state.EntityManager.GetName(hit.Entity)}");
                        //TODO BOLLOCKS ... Adding a components rearranges the memory, What if we use a enable/disable component?
                        ecb.AddComponent<SelectedUnitTag>(hit.Entity);
                        var ring = ecb.Instantiate(selectedPrefab.Value);
                        ecb.AddComponent(ring, new Parent() {
                            Value = hit.Entity
                        });
                    } else {
                        DeselectUnit(ref state, ecb, hit.Entity);
                    }
                } else {
                    DeselectAllUnits(ref state, ecb);
                }
            }
            rayCastBuffer.Clear();
        }
        
        [BurstCompile]
        private void DeselectAllUnits(ref SystemState state, EntityCommandBuffer ecb) {
            foreach (var (_, entity) in SystemAPI.Query<SelectedUnitTag>().WithEntityAccess()) {
                DeselectUnit(ref state, ecb, entity);
            }
        }
        
        [BurstCompile]
        private void DeselectUnit(ref SystemState state, EntityCommandBuffer ecb, Entity entity) {
            // Debug.Log($"Removing SelectedUnitTag from {state.EntityManager.GetName(entity)}");
            ecb.RemoveComponent<SelectedUnitTag>(entity);
            // GET THE SELECTED UNIT RING AND REMOVE IT
            if (SystemAPI.HasBuffer<Child>(entity)) {
                var childBuffer = SystemAPI.GetBuffer<Child>(entity);
                foreach (var child in childBuffer) {
                    if (SystemAPI.HasComponent<DecalComponentTag>(child.Value)) {
                        ecb.DestroyEntity(child.Value);
                    }
                }
            }
        }
        
        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }
}