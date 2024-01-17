using Selection.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
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
                Debug.Log($"Trigger Event - Collision between {state.EntityManager.GetName(triggerEvent.EntityA)} and {state.EntityManager.GetName(triggerEvent.EntityB)}");
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

        private void DebugPhysicBodies(SystemState state, PhysicsWorldSingleton pws) {
            Debug.Log($"Physic World facts - Bodies: {pws.NumBodies} Dynamic: {pws.NumDynamicBodies} Static: {pws.NumStaticBodies}");
            Debug.Log($"Bodies");
            foreach (var body in pws.Bodies) {
                var colliderType = body.Collider.IsCreated ? body.Collider.Value.Type : default;
                var belongsTo = body.Collider.IsCreated ? body.Collider.Value.GetCollisionFilter().BelongsTo : 99;
                Debug.Log($"Body {state.EntityManager.GetName(body.Entity)} C? {body.Collider.IsCreated} T:{colliderType} Bto:{belongsTo}");
            }
            Debug.Log($"Static");
            foreach (var body in pws.StaticBodies) {
                var colliderType = body.Collider.IsCreated ? body.Collider.Value.Type : default;
                var belongsTo = body.Collider.IsCreated ? body.Collider.Value.GetCollisionFilter().BelongsTo : 99;
                Debug.Log($"Static {state.EntityManager.GetName(body.Entity)} C? {body.Collider.IsCreated} T:{colliderType} Bto:{belongsTo}");
            }
            Debug.Log($"Dynamic");
            foreach (var body in pws.DynamicBodies) {
                var colliderType = body.Collider.IsCreated ? body.Collider.Value.Type : default;
                var belongsTo = body.Collider.IsCreated ? body.Collider.Value.GetCollisionFilter().BelongsTo : 99;
                Debug.Log($"Dynamic {state.EntityManager.GetName(body.Entity)} C? {body.Collider.IsCreated} T:{colliderType} Bto:{belongsTo}");
            }
        }
        //
        // private void HandleSingleSelection(ref SystemState state, DynamicBuffer<RayCastBufferComponent> rayCastBuffer, PhysicsWorld physicsWorld, EntityCommandBuffer ecb,
        //     SelectedVisualPrefabComponent selectedPrefab) {
        //     foreach (var rayCastComponent in rayCastBuffer) {
        //         if (physicsWorld.CastRay(rayCastComponent.Value, out var hit)) {
        //             if (SystemAPI.HasComponent<SelectedUnitTag>(hit.Entity)) {
        //                 if (rayCastComponent.Additive) continue;
        //                 DeselectUnit(ref state, ecb, hit.Entity);
        //             } else {
        //                 if (!rayCastComponent.Additive) {
        //                     DeselectAllUnits(ref state, ecb);    
        //                 }
        //                 SelectUnit(ref state, ecb, hit.Entity, selectedPrefab.Value);
        //             }
        //         } else {
        //             if (rayCastComponent.Additive) continue; 
        //             DeselectAllUnits(ref state, ecb);
        //             //DrawSelectionBox(ref state, ecb);
        //         }
        //     }
        //
        //     rayCastBuffer.Clear();
        // }
        //
        // private void HandleMultipleSelection(ref SystemState state, DynamicBuffer<SelectionVerticesBufferComponent> verticesBuffer, PhysicsWorld physicsWorld, EntityCommandBuffer ecb,
        //     SelectedVisualPrefabComponent selectedPrefab) {
        //     foreach (var shapeVertices in verticesBuffer) {
        //         var vertices = shapeVertices.Vertices;
        //         // var renderMeshDescription = new RenderMeshDescription(ShadowCastingMode.Off);
        //         // var mesh = new Mesh() {
        //         //     name = "Selection Mesh",
        //         //     vertices = new Vector3[] {
        //         //         vertices[0],
        //         //         vertices[1],
        //         //         vertices[2],
        //         //         vertices[3],
        //         //         vertices[4],
        //         //     },
        //         //     triangles = new[] {
        //         //         4, 0, 2,
        //         //         4, 2, 1,
        //         //         4, 1, 3,
        //         //         4, 3, 0,
        //         //         0, 1, 2,
        //         //         0, 3, 1
        //         //     },
        //         // };
        //         // var material = new UnityEngine.Material (Shader.Find("Universal Render Pipeline/Unlit")) {
        //         //     color = new Color(1f,150/255f, 0f, 145/255f)
        //         // };
        //         // var renderMeshArray = new RenderMeshArray(new[] { material }, new[] { mesh });
        //         //
        //         // //SHOULD WE USE AN ARCHETYPE?
        //         // //var entity = ecb.CreateEntity();
        //         // var entity = state.EntityManager.CreateEntity();
        //         //
        //         // //THIS ADD EVERYTHING REQUIRED TO RENDER THE MESH (BUT DOES NOT USE ECB)
        //         // RenderMeshUtility.AddComponents(entity,
        //         //     state.EntityManager,
        //         //     renderMeshDescription,
        //         //     renderMeshArray,
        //         //     MaterialMeshInfo.FromRenderMeshArrayIndices(0, 0)
        //         // );
        //         
        //         // var physicsMaterial = Unity.Physics.Material.Default;
        //         // physicsMaterial.CollisionResponse = CollisionResponsePolicy.RaiseTriggerEvents;
        //         // var collisionFilter = new CollisionFilter {
        //         //     BelongsTo = (uint) SelectionLayers.Input,
        //         //     CollidesWith = (uint) SelectionLayers.Unit
        //         // };
        //         // var selectionCollider = ConvexCollider.Create(vertices, ConvexHullGenerationParameters.Default, collisionFilter, physicsMaterial);
        //         // state.EntityManager.AddComponentData(entity, new PhysicsCollider() { Value = selectionCollider });
        //         // state.EntityManager.AddComponentData(entity, new SelectionColliderDataComponent());
        //         // state.EntityManager.AddComponentData(entity, new LocalToWorld {Value = float4x4.identity});
        //         
        //         
        //         var physicsMaterial = Unity.Physics.Material.Default;
        //         physicsMaterial.CollisionResponse = CollisionResponsePolicy.RaiseTriggerEvents;
        //         var collisionFilter = new CollisionFilter {
        //             BelongsTo = (uint) SelectionLayers.Input,
        //             CollidesWith = (uint) SelectionLayers.Unit
        //         };
        //         var selectionCollider = ConvexCollider.Create(vertices, ConvexHullGenerationParameters.Default, collisionFilter, physicsMaterial);
        //         var entity = ecb.CreateEntity();
        //         ecb.AddComponent(entity, new PhysicsCollider() { Value = selectionCollider });
        //         ecb.AddComponent(entity, new SelectionColliderDataComponent());
        //         ecb.AddComponent(entity, new LocalToWorld {Value = float4x4.identity});
        //         //vertices.Dispose();
        //     }
        //
        //     verticesBuffer.Clear();
        // }
        //
        // [BurstCompile]
        // private void SelectUnit(ref SystemState state, EntityCommandBuffer ecb, Entity entity, Entity selectedVisual) {
        //     // Debug.Log($"Adding SelectedUnitTag to {state.EntityManager.GetName(entity)}");
        //     //TODO BOLLOCKS ... Adding a components rearranges the memory, What if we use a enable/disable component?
        //     ecb.AddComponent<SelectedUnitTag>(entity);
        //     // Add the Visual
        //     var ring = ecb.Instantiate(selectedVisual);
        //     ecb.AddComponent(ring, new Parent() {
        //         Value = entity
        //     });
        // }
        //
        // [BurstCompile]
        // private void DeselectAllUnits(ref SystemState state, EntityCommandBuffer ecb) {
        //     foreach (var (_, entity) in SystemAPI.Query<SelectedUnitTag>().WithEntityAccess()) {
        //         DeselectUnit(ref state, ecb, entity);
        //     }
        // }
        //
        // [BurstCompile]
        // private void DeselectUnit(ref SystemState state, EntityCommandBuffer ecb, Entity entity) {
        //     // Debug.Log($"Removing SelectedUnitTag from {state.EntityManager.GetName(entity)}");
        //     ecb.RemoveComponent<SelectedUnitTag>(entity);
        //     // GET THE SELECTED UNIT RING AND REMOVE IT
        //     if (SystemAPI.HasBuffer<Child>(entity)) {
        //         var childBuffer = SystemAPI.GetBuffer<Child>(entity);
        //         foreach (var child in childBuffer) {
        //             if (SystemAPI.HasComponent<DecalComponentTag>(child.Value)) {
        //                 ecb.DestroyEntity(child.Value);
        //             }
        //         }
        //     }
        // }
        //
        // private void DrawSelectionBox(ref SystemState state, EntityCommandBuffer ecb) {
        //     var vertices = new NativeArray<float3>(5, Allocator.TempJob);
        //     vertices[0] = new float3(-169.7989f, -725.1414f, 659.011f);
        //     vertices[1] = new float3(883.1246f, -196.9153f, 393.2661f);
        //     vertices[2] = new float3(-0.7843962f, -207.2629f, 969.4431f);
        //     vertices[3] = new float3(699.2097f, -691.1459f, 113.4731f);
        //     vertices[4] = new float3(-10.97667f, 2.774115f, -8.567848f);
        //     
        //     var selectionCollider = ConvexCollider.Create(vertices, ConvexHullGenerationParameters.Default, new CollisionFilter() {
        //         BelongsTo = (uint) SelectionLayers.Selection,
        //         CollidesWith = (uint) SelectionLayers.Unit
        //     }, Unity.Physics.Material.Default);
        //
        //     var entity = ecb.CreateEntity();
        //     ecb.AddComponent(entity, new PhysicsCollider() { Value = selectionCollider });
        //     ecb.AddComponent(entity, new SelectionColliderDataComponent());
        //     vertices.Dispose();
        // }
        //
        // [BurstCompile]
        // public void OnDestroy(ref SystemState state) { }
    }
    
}