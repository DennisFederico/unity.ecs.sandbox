using TowerDefenseBase.Components;
using TowerDefenseEcs.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Authoring;
using Unity.Physics.Systems;
using Unity.Rendering;
using Unity.Transforms;
using ISystemStartStop = Unity.Entities.ISystemStartStop;

namespace TowerDefenseEcs.Systems {
    
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(PhysicsSystemGroup))]
    [BurstCompile]
    // ReSharper disable once RedundantExtendsListEntry
    public partial struct PlaceBuildingGhostSystem : ISystem, ISystemStartStop {
        
        //The idea of this system is to create a ghost of the building that will be placed,
        //this ghost could be used to check for collisions
        private Entity _ghostEntity;
        private BlobAssetReference<Unity.Physics.Collider> _ghostPhysicsCollider;
        private Entity _collisionVisual;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<BuildingSystemConfigData>();
            state.RequireForUpdate<PhysicsWorldSingleton>();
            state.RequireForUpdate<BuildingGhostsBufferElementData>();
            state.RequireForUpdate<GhostBuildingData>();
        }

        [BurstCompile]
        public void OnStartRunning(ref SystemState state) {
            //TODO HANDLE THIS
            var buildingSystemConfigData = SystemAPI.GetSingleton<BuildingSystemConfigData>();
            var ghosts = SystemAPI.GetSingletonBuffer<BuildingGhostsBufferElementData>();
            _ghostEntity = state.EntityManager.Instantiate(ghosts[0].Prefab);
            var collider = state.EntityManager.GetComponentData<PhysicsCollider>(_ghostEntity);
            //MUST CHANGE THE COLLIDER TAG FOR THE GHOST ENTITY OTHERWISE IT WILL COLLIDE WITH ITSELF
            var newCollider = ChangedGhostColliderFilter(collider, buildingSystemConfigData.InputSystemTag, buildingSystemConfigData.PlacingObstacles);
            state.EntityManager.SetComponentData(_ghostEntity, newCollider);
            _ghostPhysicsCollider = newCollider.Value;
            //GET THE CHILD-ENTITY THAT HAS THE COLLISION VISUAL
            _collisionVisual = GetCollisionVisualChildEntity(ref state, _ghostEntity);
        }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var dataBuffer = SystemAPI.GetSingletonBuffer<GhostBuildingData>();
            if (dataBuffer.IsEmpty) return;
            
            //Get the physics world, the building prefabs and the command buffer (all dependencies)
            var pws = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            //var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            
            //Check if the placement is valid, 1. RayCast to the ground, 2. Overlap the building shape against obstacles
            var hits = new NativeList<int>(Allocator.Temp);
            foreach (var input in dataBuffer) {
                //RayCastInput already filter for the terrain layer
                //TODO Enable/Disable visual
                if (!pws.PhysicsWorld.CastRay(input.RayInput, out var hit)) continue;
                
                //MOVE THE GHOST TO THE HIT POSITION - Not using a commandBuffer
                state.EntityManager.SetComponentData(_ghostEntity, LocalTransform.FromPositionRotation(hit.Position, input.Rotation));
                
                //Calculate the AABB of the building and overlap it against the world. The aabb is "centered" at the hit point,
                //but we don't want to offset its Y because we want it to collide with obstacles in the surface of the terrain
                //TODO WHY TO USE THE AABB HEIGHT FOR THE Y OFFSET AND THEN INCREASE THE MIN / MAS SLIGHTLY FOR BUFFER?
                var aabb = _ghostPhysicsCollider.Value.CalculateAabb(new RigidTransform {
                    pos = hit.Position,
                    rot = input.Rotation
                });
                
                //We prepare a filter to overlap the AABB against the world to check for obstacles, we don't need to check for the terrain layer
                var aabbFilter = new CollisionFilter() {
                    BelongsTo = input.RayInput.Filter.BelongsTo,
                    CollidesWith = input.ObstacleLayers.Value,
                    //CollidesWith = ~(input.RayInput.Filter.BelongsTo | input.RayInput.Filter.CollidesWith), //This also works fine
                    GroupIndex = 0
                };
                OverlapAabbInput aabbInput = new OverlapAabbInput() {
                    Aabb = aabb,
                    Filter = aabbFilter
                };
                
                //If the building overlaps with an obstacle, we skip the placement
                if (pws.OverlapAabb(aabbInput, ref hits)) {
                    ChangeCollisionVisualColor(ref state, _collisionVisual, new float4(1f, 0f, 0f, .5f));
                    //CHANGE VISUAL;
                } else {
                    ChangeCollisionVisualColor(ref state, _collisionVisual, new float4(0f, 1f, 0f, .5f));
                }
                hits.Clear();
            }
            dataBuffer.Clear();
        }

        [BurstCompile]
        public void OnStopRunning(ref SystemState state) {
            
        }
        
        [BurstCompile]
        public void OnDestroy(ref SystemState state) {

        }

        private PhysicsCollider ChangedGhostColliderFilter(PhysicsCollider collider, PhysicsCategoryTags belongsTo, PhysicsCategoryTags collidesWith) {
            var clonedCollider = collider.Value.Value.Clone();
            var collisionFilter = clonedCollider.Value.GetCollisionFilter();
            collisionFilter.BelongsTo = belongsTo.Value;
            collisionFilter.CollidesWith = collidesWith.Value;
            clonedCollider.Value.SetCollisionFilter(collisionFilter);
            return new PhysicsCollider { Value = clonedCollider };
            //state.EntityManager.SetComponentData(_ghostEntity, new PhysicsCollider { Value = clonedCollider });
        }

        private Entity GetCollisionVisualChildEntity(ref SystemState state, Entity parent) {
            var children = state.EntityManager.GetBuffer<LinkedEntityGroup>(parent);
            foreach (var child in children) {
                if (child.Value == parent) continue;
                if (state.EntityManager.HasComponent<URPMaterialPropertyBaseColor>(child.Value)) {
                    return child.Value;
                }
            }
            return Entity.Null;
        }

        private void ChangeCollisionVisualColor(ref SystemState state, Entity collisionVisualEntity, float4 color) {
            state.EntityManager.SetComponentData(collisionVisualEntity, new URPMaterialPropertyBaseColor {
                Value = color
            });
        }
    }
}