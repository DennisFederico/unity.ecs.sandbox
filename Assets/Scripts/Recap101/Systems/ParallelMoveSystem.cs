using Recap101.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Recap101.Systems {
    
    public partial struct ParallelMoveSystem : ISystem {
        
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var timeDeltaTime = SystemAPI.Time.DeltaTime;
            var moveJob = new MoveJob {
                DeltaTime = timeDeltaTime
            };
            moveJob.ScheduleParallel();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }
    
    public partial struct MoveJob : IJobEntity {
        
        public float DeltaTime;
        public void Execute(ref LocalTransform transform,
            ref NextWaypointIndexComponent nextWaypoint,
            in MoveSpeedComponent speed,
            in DynamicBuffer<WaypointsComponent> waypoints) {

            float3 direction = waypoints[nextWaypoint.Value].Value - transform.Position;
                if (math.length(direction) < 0.15f) {
                    nextWaypoint.Value = (nextWaypoint.Value + 1) % waypoints.Length;
                }

                transform.Position += math.normalize(direction) * (speed.Value * DeltaTime);
        }
    }
}