using Crowds.Aspects;
using Crowds.Components;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace Crowds.Systems.Jobs {
    public partial struct ReachedPositionJob : IJobEntity {

        [NativeDisableUnsafePtrRestriction]
        public RefRW<RandomComponent> RandomComponent;

        public void Execute(NewPositionAspect newPositionAspect) {
            newPositionAspect.TestReachedTargetPosition(RandomComponent);
        }
    }
}