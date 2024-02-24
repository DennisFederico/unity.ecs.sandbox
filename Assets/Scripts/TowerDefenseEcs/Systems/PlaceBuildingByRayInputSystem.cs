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
            state.RequireForUpdate<PlaceBuildingRayInputData>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var towerPlacementBuffer = SystemAPI.GetSingletonBuffer<PlaceBuildingRayInputData>();
            if (towerPlacementBuffer.IsEmpty) return;

            var towers = SystemAPI.GetSingletonBuffer<BuildingsBufferElementData>();
            var pws = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

            var ecbBse = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            foreach (var input in towerPlacementBuffer) {
                
                if (!pws.PhysicsWorld.CastRay(input.Value, out var hit)) continue;
                var hits = new NativeList<DistanceHit>(Allocator.Temp);
                if (pws.OverlapSphere(hit.Position + math.up(), 1f, ref hits, CollisionFilter.Default)) continue;
                var towerEntity = ecbBse.Instantiate(towers[input.TowerIndex].Prefab);
                ecbBse.SetComponent(towerEntity, LocalTransform.FromPositionRotation(hit.Position, Quaternion.identity));
            }
            towerPlacementBuffer.Clear();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {
        }
    }
}