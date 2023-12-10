# unity.dots.sandbox

Quick tests with DOTS 1.0.0

## Installation

Use package manager to install the ecs dependencies:

* Entities: com.unity.entities
* Entities Graphics: com.unity.entities.graphics
* Physics: com.unity.physics
* Mathematics: com.unity.mathematics
* Burst: com.unity.burst

## Using SystemBase

```csharp
    public partial class MoveSystem : SystemBase {
        protected override void OnUpdate() {
            var timeDeltaTime = SystemAPI.Time.DeltaTime;

            Entities.WithName("MovingEntities")
                .ForEach((ref LocalTransform transform, in MoveMeComponent moveMe) => {
                    transform.Position += new float3(0, 0, 1) * (moveMe.Speed * timeDeltaTime);
                })
                .ScheduleParallel();
        }
    }
```

But the recommendation nowadys is to use idiomatic foreach instaed of Entities.ForEach

### Idiomatic foreach

```csharp
    public partial class MoveSystem : SystemBase {
        protected override void OnUpdate() {
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
```

Notice that one difference from de idiomatic foreach versus Entities.ForEach is that the first one is not paralellized by default. To paralellize the execution of the system, you need to use the IJobForEach interface.

But you can nest idiomatic foreach, wereas Entities.ForEach is not allowed to be nested.

Idiomatic foreach is not "Bursted" by default, and needs to be used with ISystem and add the [BurstCompile] attribute.

## Using ISystem

Another recommendation is to use ISystme instaead of SystemBase

```csharp
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
```

But to paralellize the execution of the system, you need to use the IJobForEach interface

### IJobForEach

```csharp
    public struct MoveJob : IJobForEach<LocalTransform, MoveMeComponent> {
        public float DeltaTime;

        public void Execute(ref LocalTransform transform, [ReadOnly] ref MoveMeComponent moveMe) {
            transform.Position += new float3(0, 0, 1) * (moveMe.Speed * DeltaTime);
        }
    }

    public partial class MoveSystem : SystemBase {
        protected override void OnUpdate() {
            var moveJob = new MoveJob() {
                DeltaTime = SystemAPI.Time.DeltaTime
            };

            Dependency = moveJob.Schedule(this, Dependency);
        }
    }
```

