using Recap101.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Recap101.Systems {
    
    [DisableAutoCreation]
    public partial struct AnotherMoveSystem : ISystem {
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            
            var timeDeltaTime = SystemAPI.Time.DeltaTime;
            
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

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }
}