using TowerDefense.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

namespace TowerDefense.Systems {
    
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(PhysicsSystemGroup))]
    [BurstCompile]
    public partial struct TowerPlacementInputSystem : ISystem {
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<PhysicsWorldSingleton>();
            state.RequireForUpdate<TowersBufferElementData>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var towers = SystemAPI.GetSingletonBuffer<TowersBufferElementData>();
            var pws = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            var ecbBse = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            foreach (var placementInput in SystemAPI.Query<DynamicBuffer<TowerPlacementInputData>>()) {
                foreach (var input in placementInput) {
                    if (pws.PhysicsWorld.CastRay(input.Value, out var hit)) {
                        // Debug.Log($"Hit {hit.Position}");
                        var towerPos = hit.Position + math.up();
                        NativeList<DistanceHit> hits = new NativeList<DistanceHit>(Allocator.Temp);
                        if (!pws.OverlapSphere(towerPos + math.up(), 1f, ref hits, CollisionFilter.Default)) {
                            var towerEntity = ecbBse.Instantiate(towers[input.TowerIndex].Prefab);
                            ecbBse.SetComponent(towerEntity, new LocalTransform() {
                                Position = towerPos,
                                Rotation = Quaternion.identity,
                                Scale = 1f
                            });    
                        }
                    }
                }
                placementInput.Clear();
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }
}