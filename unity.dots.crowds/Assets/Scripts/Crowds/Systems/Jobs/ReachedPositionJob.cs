using Crowds.Aspects;
using Unity.Burst;
using Unity.Entities;

namespace Crowds.Systems.Jobs {
    
    [BurstCompile]
    public partial struct ReachedPositionJob : IJobEntity {

        // [NativeDisableUnsafePtrRestriction]
        // public RefRW<RandomComponent> RandomComponent;

        [BurstCompile]
        public void Execute(NewPositionAspect newPositionAspect) {
            newPositionAspect.TestReachedTargetPosition();
        }
    }
}