using sandbox;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace sandbox {
    [BurstCompile]
    public partial struct MovingISystem : ISystem {
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            new MoveJob {
                DeltaTime = SystemAPI.Time.DeltaTime
            }.ScheduleParallel(state.Dependency).Complete();

            new TestTargetPositionJob() {
                RandomRef = SystemAPI.GetSingletonRW<RandomComponent>()
            }.Run(); //RUNs on the main thread - why?
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }
}

[BurstCompile]
public partial struct MoveJob : IJobEntity {

    public float DeltaTime;
    public void Execute (MoveToPositionAspect moveToPositionAspect) {
        moveToPositionAspect.DoMove(DeltaTime);
    }
}

[BurstCompile]
public partial struct TestTargetPositionJob : IJobEntity {

    [NativeDisableUnsafePtrRestriction]
    public RefRW<RandomComponent> RandomRef;
    public void Execute (MoveToPositionAspect moveToPositionAspect) {
        moveToPositionAspect.TestTargetPosition(RandomRef);
    }
}