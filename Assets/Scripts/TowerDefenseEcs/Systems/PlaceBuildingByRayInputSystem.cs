using TowerDefenseBase.Components;
using TowerDefenseEcs.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

namespace TowerDefenseEcs.Systems {

    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(PhysicsSystemGroup))]
    [BurstCompile]
    public partial struct PlaceBuildingByRayInputSystem : ISystem {
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<PhysicsWorldSingleton>();
            state.RequireForUpdate<BuildingsBufferElementData>();
            state.RequireForUpdate<PlaceBuildingData>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            //Check of theres any command to place a building, the buffer might be empty
            var towerPlacementBuffer = SystemAPI.GetSingletonBuffer<PlaceBuildingData>();
            if (towerPlacementBuffer.IsEmpty) return;

            //Get the physics world, the building prefabs and the command buffer (all dependencies)
            var pws = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            var buildings = SystemAPI.GetSingletonBuffer<BuildingsBufferElementData>();
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            //Check if the placement is valid, 1. RayCast to the ground, 2. Overlap the building shape against obstacles
            var hits = new NativeList<int>(Allocator.Temp);
            foreach (var input in towerPlacementBuffer) {
                //RayCastInput already filter for the terrain layer
                if (!pws.PhysicsWorld.CastRay(input.RayInput, out var hit)) continue;

                //Get the actual prefab by its index and use the collider to calculate the AABB
                Entity prefab = buildings[input.BuildingIndex].Prefab;
                var physicsCollider = state.EntityManager.GetComponentData<PhysicsCollider>(prefab); //Is it possible to cache this?

                //Calculate the AABB of the building and overlap it against the world. The aabb is "centered" at the hit point,
                //but we don't want to offset its Y because we want it to collide with obstacles in the surface of the terrain
                //TODO WHY TO USE THE AABB HEIGHT FOR THE Y OFFSET AND THEN INCREASE THE MIN / MAS SLIGHTLY FOR BUFFER?
                var aabb = physicsCollider.Value.Value.CalculateAabb(new RigidTransform {
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
                if (pws.OverlapAabb(aabbInput, ref hits)) continue;

                //if (pws.OverlapSphere(hit.Position + math.up(), 1f, ref hits, CollisionFilter.Default)) continue;
                var towerEntity = ecb.Instantiate(buildings[input.BuildingIndex].Prefab);
                ecb.SetComponent(towerEntity, LocalTransform.FromPositionRotation(hit.Position, input.Rotation));
                hits.Clear();
            }
            towerPlacementBuffer.Clear();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {
        }
    }
}