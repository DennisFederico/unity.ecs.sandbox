using SimpleCrowdsSpawn.Aspects;
using Unity.Burst;
using Unity.Entities;

namespace SimpleCrowdsSpawn.Systems.Jobs {
    
    [BurstCompile]
    public partial struct MoveJob : IJobEntity {

        public float DeltaTime;
        public void Execute(MoveToPositionAspect aspect) {
            aspect.Move(DeltaTime);
        }
    }
}