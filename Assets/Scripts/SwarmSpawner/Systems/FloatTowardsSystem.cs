using SwarmSpawner.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Transforms;

namespace SwarmSpawner.Systems {
    
    [DisableAutoCreation]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial struct FloatTowardsSystem : ISystem {
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<FloatTargetAreaTag>();
        }

        //TODO WOULD IT BE WORTH IT TO MAKE THE BULK OF THIS SYSTEM A JOB?
        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var targetAreaEntity = SystemAPI.GetSingletonEntity<FloatTargetAreaTag>();
            var targetArea = SystemAPI.GetComponent<AreaComponentData>(targetAreaEntity);
            var targetTransform = SystemAPI.GetComponent<LocalTransform>(targetAreaEntity);

            // int index = 0;
            foreach (var (transform, floatTowards, velocity, mass, random) in 
                     SystemAPI.Query<
                         RefRW<LocalTransform>,
                         RefRW<FloatTowardsComponentData>,
                         RefRW<PhysicsVelocity>,
                         RefRO<PhysicsMass>, 
                         RefRW<RandomComponent>>()) {

                if (SystemAPI.Time.ElapsedTime < floatTowards.ValueRW.NextReTargetTime) return;
                
                //Set a new random point to float towards if the time has come
                floatTowards.ValueRW.NextReTargetTime = (float) (SystemAPI.Time.ElapsedTime + floatTowards.ValueRO.ReTargetRate);
                var vectorArea = targetArea.area / 2f;
                var targetPoint = random.ValueRW.Value.NextFloat3(-vectorArea, vectorArea);
                floatTowards.ValueRW.TargetPoint = targetTransform.TransformPoint(targetPoint); //because the target point is relative to the target area's transform
                
                //Calculate the direction to the target point
                var direction = math.normalize(floatTowards.ValueRO.TargetPoint - transform.ValueRO.Position);
                var moveImpulse = direction * floatTowards.ValueRO.Speed;
                velocity.ValueRW.ApplyLinearImpulse(mass.ValueRO, transform.ValueRO.Scale, moveImpulse);

                // index++;
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }
}