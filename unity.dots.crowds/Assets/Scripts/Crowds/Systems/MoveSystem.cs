using Crowds.Components;
using Crowds.Systems.Jobs;
using Unity.Burst;
using Unity.Entities;

namespace Crowds.Systems {
    public partial struct MoveSystem : ISystem {
        [BurstCompile]
        public void OnCreate(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            RefRW<RandomComponent> randomComponent = SystemAPI.GetSingletonRW<RandomComponent>();
            float timeDeltaTime = SystemAPI.Time.DeltaTime;

            var moveJobHandle = new MoveJob() {
                deltaTime = timeDeltaTime
            }.ScheduleParallel(state.Dependency);

            moveJobHandle.Complete();

            new ReachedPositionJob() {
                RandomComponent = randomComponent
            }.ScheduleParallel(moveJobHandle).Complete();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }
}