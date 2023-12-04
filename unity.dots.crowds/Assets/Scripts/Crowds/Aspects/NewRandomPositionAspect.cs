using Crowds.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Crowds.Aspects {

    [BurstCompile]
    public readonly partial struct NewRandomPositionAspect : IAspect {

        private readonly RefRO<LocalTransform> _transform;
        private readonly RefRW<Speed> _speed;
        private readonly RefRW<TargetPosition> _targetPosition;
        
        private float3 TargetPosition {
            get => _targetPosition.ValueRO.Value;
            set => _targetPosition.ValueRW.Value = value;
        }
        
        private float Speed {
            get => _speed.ValueRO.Value;
            set => _speed.ValueRW.Value = value;
        }

        [BurstCompile]
        public void TestReachedTargetPosition(RefRW<RandomSeeder> random) {
            if (!HasReachedTargetPosition()) {
                return;
            }
            TargetPosition = NewRandomPosition(random);
            Speed = random.ValueRW.NextSeed.NextFloat(1f, 3f);
        }

        [BurstCompile]
        private bool HasReachedTargetPosition() {
            return math.distancesq(_transform.ValueRO.Position, TargetPosition) < 0.1f;
        }

        [BurstCompile]
        private float3 NewRandomPosition(RefRW<RandomSeeder> random) {
            return new float3 {
                x = random.ValueRW.NextSeed.NextFloat(-25, 25f),
                y = 0f,
                z = random.ValueRW.NextSeed.NextFloat(-25f, 25f)
            };
        }
    }
}