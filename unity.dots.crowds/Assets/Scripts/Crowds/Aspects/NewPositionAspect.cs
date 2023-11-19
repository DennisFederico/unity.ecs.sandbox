using Crowds.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Crowds.Aspects {

    public readonly partial struct NewPositionAspect : IAspect {

        private readonly RefRO<LocalTransform> _transform;
        private readonly RefRW<TargetPosition> _targetPosition;
        
        private float3 TargetPosition {
            get => _targetPosition.ValueRO.Value;
            set => _targetPosition.ValueRW.Value = value;
        }

        public void TestReachedTargetPosition(RefRW<RandomComponent> random) {
            if (!HasReachedTargetPosition()) {
                return;
            }
            TargetPosition = NewRandomPosition(random);
        }

        private bool HasReachedTargetPosition() {
            return math.distancesq(_transform.ValueRO.Position, TargetPosition) < 0.1f;
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