using Selection.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;
using BoxCollider = Unity.Physics.BoxCollider;

namespace Selection.Systems {
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateBefore(typeof(PhysicsSystemGroup))]
    [BurstCompile]
    public partial struct CreateSelectionBoxColliderSystem : ISystem {
        
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<SelectionBoxBufferComponent>();
            state.RequireForUpdate<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var boxDataBuffer = SystemAPI.GetSingletonBuffer<SelectionBoxBufferComponent>();
            if (boxDataBuffer.Length == 0) return;
            var ecb = SystemAPI.GetSingleton<EndFixedStepSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            CreateSelectionCollider(ref state, boxDataBuffer, ecb);
        }
        
        [BurstCompile]
        private void CreateSelectionCollider(ref SystemState state, DynamicBuffer<SelectionBoxBufferComponent> selectionDataBuffer, EntityCommandBuffer ecb) {
            // Debug.Log("creating box selection collider");
            foreach (var selectionData in selectionDataBuffer) {
                var physicsMaterial = Unity.Physics.Material.Default;
                physicsMaterial.CollisionResponse = CollisionResponsePolicy.RaiseTriggerEvents;
                var collisionFilter = new CollisionFilter {
                    BelongsTo = selectionData.BelongsTo.Value,
                    CollidesWith = selectionData.CollidesWith.Value
                };
                
                var selectionCollider = BoxCollider.Create(new BoxGeometry() {
                    Center = selectionData.BoxCenter,
                    Size = selectionData.BoxSize,
                    Orientation = selectionData.BoxOrientation,
                    BevelRadius = 0f
                }, collisionFilter, physicsMaterial);
                
                var entity = ecb.CreateEntity();
                ecb.SetName(entity, "SelectionBox");
                ecb.AddComponent(entity, new SelectionColliderDataComponent() {
                    Additive = selectionData.Additive,
                    BelongsTo = selectionData.BelongsTo,
                    CollidesWith = selectionData.CollidesWith
                });
                ecb.AddComponent(entity, new LocalToWorld {Value = float4x4.identity});
                ecb.AddSharedComponent(entity, new PhysicsWorldIndex());
                ecb.AddComponent(entity, new PhysicsCollider() { Value = selectionCollider });
                // Debug.Log("Unit selection collider created");
            }
            selectionDataBuffer.Clear();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }
}