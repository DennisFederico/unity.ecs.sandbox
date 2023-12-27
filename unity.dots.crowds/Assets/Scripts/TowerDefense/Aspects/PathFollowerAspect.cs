using TowerDefense.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace TowerDefense.Aspects {
    public readonly partial struct PathFollowerAspect : IAspect {
        readonly RefRW<LocalTransform> transform;
        [Optional]
        readonly RefRO<MoveSpeedComponent> speed;
        readonly RefRW<NextWaypointIndexComponent> nextWaypoint;
        // [ReadOnly]
        // readonly DynamicBuffer<WaypointsComponent> waypoints;
        readonly RefRO<BlobPathAsset> path;

        public void FollowPath(float deltaTime) {
            ref var waypoints = ref path.ValueRO.Path.Value.Waypoints;
            float3 direction = waypoints[nextWaypoint.ValueRO.Value] - transform.ValueRO.Position;
            if (math.length(direction) < 0.15f) {
                nextWaypoint.ValueRW.Value = (nextWaypoint.ValueRO.Value + 1) % waypoints.Length;
            }

            var aSpeed = speed.IsValid ? speed.ValueRO.Value : 1f;
            transform.ValueRW.Position += math.normalize(direction) * (aSpeed * deltaTime);
            transform.ValueRW.Rotation = quaternion.LookRotation(direction, math.up());
        }
        
        public bool IsAtEndOfPath() {
            ref var waypoints = ref path.ValueRO.Path.Value.Waypoints;
            return math.distance(transform.ValueRO.Position, waypoints[^1]) < 0.15f;
        }
    }
}