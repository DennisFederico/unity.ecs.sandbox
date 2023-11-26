using Crowds.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Crowds.Aspects {

    public readonly partial struct NewPositionAspect : IAspect {

        private readonly RefRO<LocalTransform> _transform;
        private readonly RefRW<TargetPosition> _targetPosition;
        private readonly RefRW<Speed> _speed;
        
        private float3 TargetPosition {
            get => _targetPosition.ValueRO.Value;
            set => _targetPosition.ValueRW.Value = value;
        }
        
        private float Speed {
            get => _speed.ValueRO.Value;
            set => _speed.ValueRW.Value = value;
        }

        public void TestReachedTargetPosition(RefRW<RandomComponent> random) {
            if (!HasReachedTargetPosition()) {
                return;
            }
            TargetPosition = NewRandomPosition(random);
            Speed = random.ValueRW.Value.NextFloat(1f, 3f);
        }

        private bool HasReachedTargetPosition() {
            return math.distancesq(_transform.ValueRO.Position, TargetPosition) < 0.1f;
        }

        private float3 NewRandomPosition(RefRW<RandomComponent> random) {
            return Utils.Utils.NewRandomPosition(random.ValueRW.Value);
        }
    }
}