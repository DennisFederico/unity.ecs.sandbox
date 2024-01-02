using PlayerCamera.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Transforms;

namespace PlayerCamera.Systems {
    
    [DisableAutoCreation]
    [UpdateAfter(typeof(FixedStepSimulationSystemGroup))]
    public partial struct MovingSystem : ISystem {
        [BurstCompile]
        public void OnCreate(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            //Get Inputs - Old Input System
            var forward = UnityEngine.Input.GetAxis("Vertical") * SystemAPI.Time.DeltaTime;
            var rotate = UnityEngine.Input.GetAxis("Horizontal") * SystemAPI.Time.DeltaTime;

            foreach (var (moveData,
                         transform,
                         velocity,
                         mass)
                     in SystemAPI.Query<
                         RefRO<MovableComponentData>,
                         RefRW<LocalTransform>,
                         RefRW<PhysicsVelocity>,
                         RefRW<PhysicsMass>>()) {
                //Apply forces to move the object
                var forwardImpulse = transform.ValueRW.Forward() * forward * moveData.ValueRO.MoveSpeed;
                velocity.ValueRW.ApplyLinearImpulse(mass.ValueRO, forwardImpulse);

                var rotationImpulse = rotate * moveData.ValueRO.RotateSpeed;
                velocity.ValueRW.ApplyAngularImpulse(mass.ValueRO, new Unity.Mathematics.float3(0, rotationImpulse, 0));
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }
}