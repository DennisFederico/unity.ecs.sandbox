using Crowds.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Crowds.Aspects {
    
    public readonly partial struct MoveToPositionAspect : IAspect {
 
        //private readonly Entity _self;
        private readonly RefRW<LocalTransform> _transform;
        private readonly RefRO<Speed> _speed;
        private readonly RefRO<TargetPosition> _targetPosition;

        private float Speed => _speed.ValueRO.Value;

        private float3 TargetPosition => _targetPosition.ValueRO.Value;

        //TODO SPLIT THE LOGIC FOR NEW POSITION IN ANOTHER SYSTEM TO AVOID PASSING THE RANDOM COMPONENT HERE
        public void Move(float deltaTime) {
            if (HasReachedTargetPosition()) {
                return;
            }
            _transform.ValueRW.Position += math.normalize(TargetPosition - _transform.ValueRO.Position) * (Speed * deltaTime);
        }
        
        private bool HasReachedTargetPosition() {
            return math.distancesq(_transform.ValueRO.Position, TargetPosition) < 0.1f;
        }
    }
}