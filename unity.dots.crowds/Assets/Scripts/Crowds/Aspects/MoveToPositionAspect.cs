using Crowds.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Crowds.Aspects {
    public readonly partial struct MoveToPositionAspect : IAspect {
 
        private readonly Entity _self;
        private readonly RefRW<LocalTransform> _transform;
        private readonly RefRW<Speed> _speed;
        private readonly RefRW<TargetPosition> _targetPosition;

        public float Speed {
            get => _speed.ValueRO.Value;
            set => _speed.ValueRW.Value = value;
        }

        public float3 TargetPosition {
            get => _targetPosition.ValueRO.Value;
            set => _targetPosition.ValueRW.Value = value;
        }

        //TODO SPLIT THE LOGIC FOR NEW POSITION IN ANOTHER SYSTEM TO AVOID PASSING THE RANDOM COMPONENT HERE
        public void Move(float deltaTime, RefRW<RandomComponent> random) {
            _transform.ValueRW.Position += math.normalize(TargetPosition - _transform.ValueRO.Position) * (Speed * deltaTime);
            if (math.distancesq(_transform.ValueRO.Position, TargetPosition) < 0.1f) {
                TargetPosition = NewRandomPosition(random);
            }
        }

        private float3 NewRandomPosition(RefRW<RandomComponent> random) {
            return new float3 {
                x = random.ValueRW.Value.NextFloat(-15f, 15f),
                y = 0f,
                z = random.ValueRW.Value.NextFloat(-15f, 15f)
            };
        }
    }
}