using Crowds.Aspects;
using Crowds.Components;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace Crowds.Systems.Jobs {
    
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