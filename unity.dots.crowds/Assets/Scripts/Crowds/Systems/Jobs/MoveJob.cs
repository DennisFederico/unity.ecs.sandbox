using Crowds.Aspects;
using Unity.Entities;

namespace Crowds.Systems.Jobs {
    
    public partial struct MoveJob : IJobEntity {

        public float deltaTime;
        public void Execute(MoveToPositionAspect aspect) {
            aspect.Move(deltaTime);
        }
    }
}