using Recap101.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Recap101.Systems {
    
    public partial class MoveSystem : SystemBase {
        protected override void OnUpdate() {
            var timeDeltaTime = SystemAPI.Time.DeltaTime;

            // Entities
            //     .WithName("MovingEntities")
            //     //.WithDeferredPlaybackSystem<EndSimulationEntityCommandBufferSystem>()
            //     .ForEach((ref LocalTransform transform,
            //         ref NextWaypointIndexComponent nextWaypoint,
            //         in MoveSpeedComponent speed,
            //         in DynamicBuffer<WaypointsComponent> waypoints) => {
            //         
            //         float3 direction = waypoints[nextWaypoint.Value].Value - transform.Position;
            //         if (math.length(direction) < 0.15f) {
            //             nextWaypoint.Value = (nextWaypoint.Value + 1) % waypoints.Length;
            //         }
            //
            //         transform.Position += math.normalize(direction) * (speed.Value * timeDeltaTime);
            //     })
            //     .ScheduleParallel();

            foreach (var (transform, speed, nextWaypoint, waypoints) in
                     SystemAPI.Query<
                         RefRW<LocalTransform>,
                         RefRO<MoveSpeedComponent>,
                         RefRW<NextWaypointIndexComponent>,
                         DynamicBuffer<WaypointsComponent>>()) {
                float3 direction = waypoints[nextWaypoint.ValueRO.Value].Value - transform.ValueRO.Position;
                if (math.length(direction) < 0.15f) {
                    nextWaypoint.ValueRW.Value = (nextWaypoint.ValueRO.Value + 1) % waypoints.Length;
                }

                transform.ValueRW.Position += math.normalize(direction) * (speed.ValueRO.Value * timeDeltaTime);
            }
        }
    }
}