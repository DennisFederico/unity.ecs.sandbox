using Selection.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

namespace Selection.Systems {
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateBefore(typeof(PhysicsSystemGroup))]
    [BurstCompile]
    public partial struct CreateSelectionPrismColliderSystem : ISystem {
        
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<SelectionVerticesBufferComponent>();
            state.RequireForUpdate<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var verticesBuffer = SystemAPI.GetSingletonBuffer<SelectionVerticesBufferComponent>();
            if (verticesBuffer.Length == 0) return;
            var ecb = SystemAPI.GetSingleton<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            CreateSelectionCollider(ref state, verticesBuffer, ecb);
        }
        
        [BurstCompile]
        private void CreateSelectionCollider(ref SystemState state, DynamicBuffer<SelectionVerticesBufferComponent> selectionDataBuffer, EntityCommandBuffer ecb) {
            // Debug.Log("creating selection collider");
            foreach (var selectionData in selectionDataBuffer) {
                var physicsMaterial = Unity.Physics.Material.Default;
                physicsMaterial.CollisionResponse = CollisionResponsePolicy.RaiseTriggerEvents;
                var collisionFilter = new CollisionFilter {
                    BelongsTo = selectionData.BelongsTo.Value,
                    CollidesWith = selectionData.CollidesWith.Value
                };
                var selectionCollider = ConvexCollider.Create(selectionData.Vertices, ConvexHullGenerationParameters.Default, collisionFilter, physicsMaterial);
                
                var entity = ecb.CreateEntity();
                ecb.SetName(entity, "Selection");
                ecb.AddComponent(entity, new SelectionColliderDataComponent() {
                    Additive = selectionData.Additive,
                    BelongsTo = selectionData.BelongsTo,
                    CollidesWith = selectionData.CollidesWith
                });
                ecb.AddComponent(entity, new LocalToWorld {Value = float4x4.identity});
                ecb.AddSharedComponent(entity, new PhysicsWorldIndex());
                ecb.AddComponent(entity, new PhysicsCollider() { Value = selectionCollider });
                // ReSharper disable once PossiblyImpureMethodCallOnReadonlyVariable
                selectionData.Vertices.Dispose(); //TODO Dispose the vertices if TEMP is not enough
                // Debug.Log("Unit selection collider created");
            }
            selectionDataBuffer.Clear();
        }
        
        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }
}