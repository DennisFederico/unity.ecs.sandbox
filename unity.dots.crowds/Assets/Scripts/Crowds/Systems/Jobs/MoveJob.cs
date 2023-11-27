using Crowds.Aspects;
using Unity.Burst;
using Unity.Entities;

namespace Crowds.Systems.Jobs {
    
    [BurstCompile]
    public partial struct MoveJob : IJobEntity {

        public float DeltaTime;
        public void Execute(MoveToPositionAspect aspect) {
            aspect.Move(DeltaTime);
        }
    }
}