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

## Using ISystem and IJobEntity

```csharp


```

