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

        //The idea of this system is to create a ghost of the building that will be placed, this ghost could be used to check for collisions
        private Entity _ghostEntity;
        private BlobAssetReference<Unity.Physics.Collider> _ghostPhysicsCollider;
        private Entity _collisionVisual;
        private int _currentGhostIndex;

        private PhysicsCategoryTags _obstacleLayers;
        private PhysicsCategoryTags _buildingSystemLayer;

        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<BuildingSystemConfigData>();
            state.RequireForUpdate<PhysicsWorldSingleton>();
            state.RequireForUpdate<BuildingRegistryTag>();
            state.RequireForUpdate<BuildingGhostsBufferElementData>();
            state.RequireForUpdate<GhostBuildingData>();
        }

        [BurstCompile]
        public void OnStartRunning(ref SystemState state) {
            _currentGhostIndex = -1;
            _ghostEntity = Entity.Null;

            var buildingSystemConfigData = SystemAPI.GetSingleton<BuildingSystemConfigData>();
            _obstacleLayers = buildingSystemConfigData.PlacingObstacles;
            _buildingSystemLayer = buildingSystemConfigData.InputSystemTag;

            //TODO Cache the ghost entities and the collision visuals for each building type (as disabled by default)?
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var dataBuffer = SystemAPI.GetSingletonBuffer<GhostBuildingData>();
            if (dataBuffer.IsEmpty) return;

            //Get the physics world, the building prefabs and the command buffer (all dependencies)
            var pws = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            //Use of a command buffer if entity "destroy" or "enable/disable" is required - Optional use an adhoc EntityCommandBuffer 
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            //Check if the placement is valid, 1. RayCast to the ground, 2. Overlap the building shape against obstacles
            var hits = new NativeList<int>(Allocator.Temp);

            foreach (var input in dataBuffer) {
                //Changing the ghost?
                if (_currentGhostIndex != input.BuildingIndex) {
                    _currentGhostIndex = input.BuildingIndex;
                    var buildingGhosts = SystemAPI.GetSingletonBuffer<BuildingGhostsBufferElementData>();
                    if (_currentGhostIndex < 0 || _currentGhostIndex >= buildingGhosts.Length) {
                        _currentGhostIndex = -1;
                        ecb.DestroyEntity(_ghostEntity);
                        _ghostEntity = Entity.Null;
                        return; //Nothing else to do
                    }
                    var ghostPrefab = buildingGhosts[_currentGhostIndex].Prefab;
                    ChangeGhostEntity(ref state, ghostPrefab, ecb);
                }

                if (_ghostEntity == Entity.Null) continue;

                //RayCastInput already filter for the terrain layer
                if (!pws.PhysicsWorld.CastRay(input.RayInput, out var hit)) {
                    if (!state.EntityManager.HasComponent<DisableRendering>(_ghostEntity)) {
                        ApplyComponentIncludingChildren<DisableRendering>(ref state, _ghostEntity, ecb);
                    }
                    continue;
                }

                //NOTE. At this point physics simulation as already been done, but the ghost should not care as it in itself is not an obstacle for others
                if (state.EntityManager.HasComponent<DisableRendering>(_ghostEntity)) {
                    RemoveComponentIncludingChildren<DisableRendering>(ref state, _ghostEntity, ecb);
                }

                //MOVE THE GHOST TO THE HIT POSITION - Not using a commandBuffer since its not a structural change
                state.EntityManager.SetComponentData(_ghostEntity, LocalTransform.FromPositionRotation(hit.Position, input.Rotation));

                //Calculate the AABB of the building and overlap it against the world. The aabb is "centered" at the hit point,
                //but we don't want to offset its Y because we want it to collide with obstacles in the surface of the terrain (the path)
                //We could use the height from the prefab aabb and adjust the hit position Y and increase the MIN/MAX slightly for collision?
                var aabb = _ghostPhysicsCollider.Value.CalculateAabb(new RigidTransform {
                    pos = hit.Position,
                    rot = input.Rotation
                });
                //We prepare a filter to overlap the AABB against the world to check for obstacles, we don't need to check for the terrain layer
                var aabbFilter = new CollisionFilter() {
                    BelongsTo = input.RayInput.Filter.BelongsTo,
                    CollidesWith = input.ObstacleLayers.Value,
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

        private void ChangeGhostEntity(ref SystemState state, Entity ghostPrefab, EntityCommandBuffer ecb) {
            //state.EntityManager.DestroyEntity(_ghostEntity);
            if (_ghostEntity != Entity.Null) ecb.DestroyEntity(_ghostEntity);
            _ghostEntity = state.EntityManager.Instantiate(ghostPrefab);
            var collider = state.EntityManager.GetComponentData<PhysicsCollider>(_ghostEntity);
            var newCollider = ChangedGhostColliderFilter(collider, _buildingSystemLayer, _obstacleLayers);
            state.EntityManager.SetComponentData(_ghostEntity, newCollider);
            _ghostPhysicsCollider = newCollider.Value;
            _collisionVisual = GetCollisionVisualChildEntity(ref state, _ghostEntity);
        }

        private PhysicsCollider ChangedGhostColliderFilter(PhysicsCollider collider, PhysicsCategoryTags belongsTo, PhysicsCategoryTags collidesWith) {
            var clonedCollider = collider.Value.Value.Clone();
            var collisionFilter = clonedCollider.Value.GetCollisionFilter();
            collisionFilter.BelongsTo = belongsTo.Value;
            collisionFilter.CollidesWith = collidesWith.Value;
            clonedCollider.Value.SetCollisionFilter(collisionFilter);
            return new PhysicsCollider { Value = clonedCollider };
        }

        private Entity GetCollisionVisualChildEntity(ref SystemState state, Entity parent) {
            var children = state.EntityManager.GetBuffer<LinkedEntityGroup>(parent);
            foreach (var child in children) {
                if (child.Value == parent) continue;
                //TODO Change to a component lookup?
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
        
        private void ApplyComponentIncludingChildren<T> (ref SystemState state, Entity entity, EntityCommandBuffer ecb) where T : unmanaged, IComponentData {
            var children = state.EntityManager.GetBuffer<LinkedEntityGroup>(entity);
            foreach (var child in children) {
                ecb.AddComponent<T>(child.Value);
            }
        }
        
        private void RemoveComponentIncludingChildren<T> (ref SystemState state, Entity entity, EntityCommandBuffer ecb) where T : unmanaged, IComponentData {
            var children = state.EntityManager.GetBuffer<LinkedEntityGroup>(entity);
            foreach (var child in children) {
                ecb.RemoveComponent<T>(child.Value);
            }
        }
    }
}