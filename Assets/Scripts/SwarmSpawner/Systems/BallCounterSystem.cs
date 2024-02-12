using System;
using SwarmSpawner.Components;
using Unity.Entities;

namespace SwarmSpawner.Systems {
    
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public partial class BallCounterSystem : SystemBase {
        
        public event Action<int> OnBallCountChanged;
        private int _currentBallCount;
        private EntityQuery _ballCountQuery;

        protected override void OnCreate() {
            base.OnCreate();
            RequireForUpdate<FloatTowardsComponentData>();
            _ballCountQuery = SystemAPI.QueryBuilder()
                .WithAll<FloatTowardsComponentData>()
                .Build();
            _currentBallCount = 0;
        }

        protected override void OnUpdate() {
            var ballCount = _ballCountQuery.CalculateEntityCount();
            if (ballCount != _currentBallCount) {
                _currentBallCount = ballCount;
                OnBallCountChanged?.Invoke(_currentBallCount);
            }
        }
    }
}