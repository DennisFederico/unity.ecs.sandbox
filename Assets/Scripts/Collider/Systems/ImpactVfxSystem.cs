using Collider.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;

namespace Collider.Systems {
    
    public partial struct ImpactVfxSystem : ISystem {
        
        // [BurstCompile]
        // private struct TriggerJob : ITriggerEventsJob {
        //     public NativeReference<int> NumTriggerEvents;
        //     public void Execute(TriggerEvent collisionEvent) {
        //         NumTriggerEvents.Value++;
        //     }
        // }

        private ComponentLookup<ImpactVfxComponent> _vfxLookup;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<PhysicsWorldSingleton>();
            state.RequireForUpdate<SimulationSingleton>();
            state.RequireForUpdate<ImpactVfxComponent>();
            _vfxLookup = SystemAPI.GetComponentLookup<ImpactVfxComponent>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            state.CompleteDependency();
            
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);
            var pws = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            var simulationSingleton = SystemAPI.GetSingleton<SimulationSingleton>();
            var simulation = simulationSingleton.AsSimulation();

            _vfxLookup.Update(ref state);

            foreach (var triggerEvent in simulation.TriggerEvents) {
                var collisionFilterA = pws.Bodies[triggerEvent.BodyIndexA].Collider.Value.GetCollisionFilter();
                var collisionFilterB = pws.Bodies[triggerEvent.BodyIndexB].Collider.Value.GetCollisionFilter();
                if (!CollisionFilter.IsCollisionEnabled(collisionFilterA, collisionFilterB)) return;

                int bodyIndexSphere;
                int bodyIndexCube;
                CollisionFilter collisionFilter;
                Entity vfxPrefab;
                
                if (_vfxLookup.HasComponent(triggerEvent.EntityA)) {
                    bodyIndexSphere = triggerEvent.BodyIndexA;
                    bodyIndexCube = triggerEvent.BodyIndexB;
                    collisionFilter = collisionFilterA;
                    vfxPrefab = _vfxLookup[triggerEvent.EntityA].VfxPrefab;
                } else {
                    bodyIndexSphere = triggerEvent.BodyIndexB;
                    bodyIndexCube = triggerEvent.BodyIndexA;
                    collisionFilter = collisionFilterB;
                    vfxPrefab = _vfxLookup[triggerEvent.EntityB].VfxPrefab;
                }
                
                PointDistanceInput distanceInput = new PointDistanceInput {
                    Position = pws.Bodies[bodyIndexSphere].WorldFromBody.pos,
                    MaxDistance = 10f,
                    Filter = collisionFilter
                };
                
                if (pws.Bodies[bodyIndexCube].CalculateDistance(distanceInput, out var distanceHit)) {
                    var vfx = ecb.Instantiate(vfxPrefab);
                    ecb.AddComponent(vfx,LocalTransform.FromPosition(distanceHit.Position));
                }
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {

        }
    }
}