using Spawner.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Transforms;
using UnityEngine;

namespace Spawner.Systems {
    
    [DisableAutoCreation]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial struct MoveByPhysicsSystem : ISystem {
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var forward = Input.GetAxis("Vertical") * SystemAPI.Time.DeltaTime;
            var rotate = Input.GetAxis("Horizontal") * SystemAPI.Time.DeltaTime;
            
            //Use a query to move all the entities with the proper component data
            foreach (var (entity, transform, velocity, mass) in SystemAPI.Query<
                         RefRO<MovingComponentData>, 
                         RefRW<LocalTransform>, 
                         RefRW<PhysicsVelocity>, 
                         RefRW<PhysicsMass>>()) {
                
                //Use force to move the object
                var moveImpulse = transform.ValueRW.Forward() * forward * entity.ValueRO.moveSpeed;
                // velocity.ValueRW.ApplyLinearImpulse(mass.ValueRO, moveImpulse);
                velocity.ValueRW.Linear = moveImpulse;
                //Use force to rotate the object
                var rotateImpulse = rotate * entity.ValueRO.rotateSpeed; 
                velocity.ValueRW.ApplyAngularImpulse(mass.ValueRO, new float3(0, rotateImpulse, 0));
                //velocity.ValueRW.Angular = rotateImpulse;
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }
}