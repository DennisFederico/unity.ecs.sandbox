using System;
using Selection.Components;
using Unity.Burst;
using Unity.Entities;

namespace Selection.Systems {

    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    [BurstCompile]
    public partial class SelectedCountEventSystem : SystemBase {

        public event Action<int> OnSelectedCountChanged;
        private int _currentSelectedCount;
        private EntityQuery _selectedUnitsQuery;

        protected override void OnCreate() {
            base.OnCreate();
            RequireForUpdate<SelectedUnitTag>();
            _selectedUnitsQuery = SystemAPI.QueryBuilder()
                .WithAll<SelectedUnitTag>()
                .Build();
            _currentSelectedCount = 0;
        }

        protected override void OnUpdate() {
            var selectedCount = _selectedUnitsQuery.CalculateEntityCount();
            if (selectedCount != _currentSelectedCount) {
                _currentSelectedCount = selectedCount;
                OnSelectedCountChanged?.Invoke(_currentSelectedCount);
            }
        }
    }
}