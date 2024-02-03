using SimpleCrowdsSpawn.Aspects;
using SimpleCrowdsSpawn.Components;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Utils.Narkdagas.Ecs;

namespace SimpleCrowdsSpawn.Systems {

    public partial struct MoveSystem : ISystem {
        
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<RandomSeeder>();
            state.RequireForUpdate<CrowdMemberTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            RefRW<RandomSeeder> randomSeeder = SystemAPI.GetSingletonRW<RandomSeeder>();
            float timeDeltaTime = SystemAPI.Time.DeltaTime;

            state.Dependency = new MoveJob() {
                DeltaTime = timeDeltaTime
            }.ScheduleParallel(state.Dependency);
            
            state.Dependency = new ReachedPositionRandomJob {
                RandomComponent = randomSeeder
            }.ScheduleParallel(state.Dependency);
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }
    
    [BurstCompile]
    public partial struct MoveJob : IJobEntity {

        public float DeltaTime;
        public void Execute(MoveToPositionAspect aspect) {
            aspect.Move(DeltaTime);
        }
    }
    
    [BurstCompile]
    public partial struct ReachedPositionRandomJob : IJobEntity {

        [NativeDisableUnsafePtrRestriction]
        public RefRW<RandomSeeder> RandomComponent;

        [BurstCompile]
        public void Execute(NewRandomPositionAspect newPositionAspect) {
            newPositionAspect.TestReachedTargetPosition(RandomComponent);
        }
    }
}