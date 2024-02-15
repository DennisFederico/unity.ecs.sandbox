using TowerDefense.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace TowerDefense.Aspects {
    public readonly partial struct PathFollowerAspect : IAspect {
        
        private readonly RefRW<LocalTransform> _transform;

        public quaternion Rotation {
            get => _transform.ValueRO.Rotation;
            set => _transform.ValueRW.Rotation = value;
        }

        public float3 Position {
            get => _transform.ValueRO.Position;
            set => _transform.ValueRW.Position = value;
        }

        private readonly RefRO<MoveSpeedComponent> _speed;
        public float Speed => _speed.ValueRO.Value;
        
        private readonly RefRW<NextWaypointIndexComponent> _nextWaypoint;
        private readonly RefRO<WaypointsAsset> _path;

        public void FollowPath(float deltaTime) {
            ref var waypoints = ref _path.ValueRO.Path.Value.Waypoints;
            var direction = waypoints[_nextWaypoint.ValueRO.Value] - _transform.ValueRO.Position;
            if (math.length(direction) < 0.15f) {
                _nextWaypoint.ValueRW.Value = (_nextWaypoint.ValueRO.Value + 1) % waypoints.Length;
            }

            var aSpeed = _speed.IsValid ? _speed.ValueRO.Value : 1f;
            _transform.ValueRW.Position += math.normalize(direction) * (aSpeed * deltaTime);
            _transform.ValueRW.Rotation = quaternion.LookRotation(direction, math.up());
        }
        
        public bool IsAtEndOfPath() {
            ref var waypoints = ref _path.ValueRO.Path.Value.Waypoints;
            return math.distance(_transform.ValueRO.Position, waypoints[^1]) < 0.15f;
        }
    }
}