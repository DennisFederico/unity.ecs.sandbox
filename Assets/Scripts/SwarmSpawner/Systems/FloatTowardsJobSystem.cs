using SwarmSpawner.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Transforms;

namespace SwarmSpawner.Systems {

    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial struct FloatTowardsJobSystem : ISystem {
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<FloatTargetAreaTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var targetAreaEntity = SystemAPI.GetSingletonEntity<FloatTargetAreaTag>();
            var targetArea = SystemAPI.GetComponent<AreaComponentData>(targetAreaEntity);
            var targetAreaTransform = SystemAPI.GetComponent<LocalTransform>(targetAreaEntity);

            new FloatingTowardsSystemJob {
                ElapsedTime = SystemAPI.Time.ElapsedTime,
                TargetArea = targetArea,
                TargetAreaTransform = targetAreaTransform
            }.ScheduleParallel();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }

    [BurstCompile]
    public partial struct FloatingTowardsSystemJob : IJobEntity {
        [ReadOnly] public AreaComponentData TargetArea;
        public LocalTransform TargetAreaTransform;
        public double ElapsedTime;

        [BurstCompile]
        private void Execute([ChunkIndexInQuery] int chunkIndex, [EntityIndexInChunk] int entityIndex,
            ref FloatTowardsComponentData floatTowards, ref LocalTransform localTransform,
            ref PhysicsVelocity velocity, ref PhysicsMass mass) {
            
            // If the time hasn't come to re-target, return
            if (ElapsedTime < floatTowards.NextReTargetTime) return;
            
            // Initialise the Random number generator if it hasn't been initialised yet
            if(floatTowards.Random.state == 0) {
                floatTowards.Random = new Random((uint) (ElapsedTime * 100 + chunkIndex + entityIndex + 1));
            }
            
            // Set a new random point to float towards if the time has come
            floatTowards.NextReTargetTime = (float)(ElapsedTime + floatTowards.ReTargetRate);
            var point = floatTowards.Random.NextFloat3(-TargetArea.area / 2f, TargetArea.area / 2f);
            floatTowards.TargetPoint = TargetAreaTransform.TransformPoint(point);
            
            // move towards the target point
            var direction = math.normalize(floatTowards.TargetPoint - localTransform.Position);
            var moveImpulse = direction * floatTowards.Speed;
            velocity.ApplyLinearImpulse(mass, localTransform.Scale, moveImpulse);
        }
    }
}