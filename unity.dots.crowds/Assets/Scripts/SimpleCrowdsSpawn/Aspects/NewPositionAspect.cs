using SimpleCrowdsSpawn.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace SimpleCrowdsSpawn.Aspects {

    [BurstCompile]
    public readonly partial struct NewPositionAspect : IAspect {

        private readonly RefRO<LocalTransform> _transform;
        private readonly RefRW<RandomComponent> _random;
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
        public void TestReachedTargetPosition() {
            if (!HasReachedTargetPosition()) {
                return;
            }
            TargetPosition = NewRandomPosition();
            Speed = _random.ValueRW.Value.NextFloat(1f, 3f);
        }

        [BurstCompile]
        private bool HasReachedTargetPosition() {
            return math.distancesq(_transform.ValueRO.Position, TargetPosition) < 0.1f;
        }

        [BurstCompile]
        private float3 NewRandomPosition() {
            return new float3 {
                x = _random.ValueRW.Value.NextFloat(-25, 25f),
                y = 0f,
                z = _random.ValueRW.Value.NextFloat(-25f, 25f)
            };
        }
    }
}