using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace sandbox {
    public readonly partial struct MoveToPositionAspect : IAspect {
        private readonly Entity _entity;
        private readonly RefRW<LocalTransform> _transformAspect;
        private readonly RefRO<Speed> _speed;
        private readonly RefRW<TargetPosition> _targetPosition;
        
        public void DoMove(float deltaTime) {
            //Direction
            float3 direction = math.normalize(_targetPosition.ValueRW.Value - _transformAspect.ValueRW.Position);
            //Move
            _transformAspect.ValueRW.Position += direction * (deltaTime * _speed.ValueRO.Value);
        }
        
        public void TestTargetPosition(RefRW<RandomComponent> randomRef) {
            float reachedDistance = 0.1f;
            if (math.distancesq(_transformAspect.ValueRW.Position, _targetPosition.ValueRW.Value) < reachedDistance) {
                //New Random Target Position
                _targetPosition.ValueRW.Value = GetRandomPosition(randomRef);
            }
        }
        
        private float3 GetRandomPosition(RefRW<RandomComponent> randomRef) {
            return new float3(randomRef.ValueRW.Random.NextFloat(-15f, 15f), 0, randomRef.ValueRW.Random.NextFloat(-10f, 10f));
        }
    }
}