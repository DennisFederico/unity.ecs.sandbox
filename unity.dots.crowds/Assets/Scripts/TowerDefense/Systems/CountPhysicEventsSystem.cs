using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;

namespace TowerDefense.Systems {
    
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(PhysicsSystemGroup))] // We are updating after `PhysicsSimulationGroup` - this means that we will get the events of the current frame.
    //[UpdateAfter(typeof(PhysicsSimulationGroup))] // We are updating after `PhysicsSimulationGroup` - this means that we will get the events of the current frame.
    
    public partial struct CountPhysicEventsSystem : ISystem {
        
        [BurstCompile]
        private struct CountNumTriggerEvents : ITriggerEventsJob {
            public NativeReference<int> NumTriggerEvents;
            public void Execute(TriggerEvent collisionEvent) {
                NumTriggerEvents.Value++;
            }
        }
        
        [BurstCompile]
        private struct CountNumCollisionEvents : ICollisionEventsJob {
            public NativeReference<int> NumCollisionEvents;
            public void Execute(CollisionEvent collisionEvent) {
                NumCollisionEvents.Value++;
            }
        }
        
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<SimulationSingleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            NativeReference<int> numTriggerEvents = new NativeReference<int>(0, Allocator.TempJob);
            NativeReference<int> numCollisionEvents = new NativeReference<int>(0, Allocator.TempJob);
            
            var triggerHandle = new CountNumTriggerEvents {
                NumTriggerEvents = numTriggerEvents
            }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);

            var collisionHandle = new CountNumCollisionEvents {
                NumCollisionEvents = numCollisionEvents
            }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);

            state.Dependency.Complete();
            triggerHandle.Complete();
            collisionHandle.Complete();
            // Debug.Log($"numTriggerEvents: {numTriggerEvents.Value}");
            // Debug.Log($"numCollisionEvents: {numCollisionEvents.Value}");
            numTriggerEvents.Dispose();
            numCollisionEvents.Dispose();

            // foreach (var triggers in SystemAPI.GetSingleton<SimulationSingleton>().AsSimulation().TriggerEvents) {
            //     
            // }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }
}
