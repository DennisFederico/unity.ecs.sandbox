using Recap101.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Recap101.Systems {
    public partial class MoveSystem : SystemBase {
        protected override void OnUpdate() {
            var timeDeltaTime = SystemAPI.Time.DeltaTime;

            Entities
                .WithName("MovingEntities")
                //.WithDeferredPlaybackSystem<EndSimulationEntityCommandBufferSystem>()
                .ForEach((ref LocalTransform transform, 
                    ref NextWaypointIndexComponent nextWaypoint, 
                    in MoveSpeed speed,
                    in DynamicBuffer<WaypointsComponent> waypoints) => {
                    float3 direction = waypoints[nextWaypoint.Value].Value - transform.Position;
                    if (math.length(direction) < 0.15f) {
                        nextWaypoint.Value = (nextWaypoint.Value + 1) % waypoints.Length;
                    }
                    transform.Position += math.normalize(direction) * (speed.Value * timeDeltaTime);
                    
                })
                .ScheduleParallel();
        }
    }
}