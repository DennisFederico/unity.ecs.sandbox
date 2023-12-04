using Crowds.Components;
using Crowds.Systems.Jobs;
using Unity.Burst;
using Unity.Entities;

namespace Crowds.Systems {
    [DisableAutoCreation]
    public partial struct MoveSystem : ISystem {
        [BurstCompile]
        public void OnCreate(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            RefRW<RandomSeeder> randomSeeder = SystemAPI.GetSingletonRW<RandomSeeder>();
            float timeDeltaTime = SystemAPI.Time.DeltaTime;

            var moveJobHandle = new MoveJob() {
                DeltaTime = timeDeltaTime
            }.ScheduleParallel(state.Dependency);

            moveJobHandle.Complete();

            new ReachedPositionRandomJob() {
                RandomComponent = randomSeeder
            }.ScheduleParallel(moveJobHandle).Complete();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }
}