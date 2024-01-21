using SimpleCrowdsSpawn.Aspects;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Utils.Narkdagas.Ecs;

namespace SimpleCrowdsSpawn.Systems.Jobs {
    
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