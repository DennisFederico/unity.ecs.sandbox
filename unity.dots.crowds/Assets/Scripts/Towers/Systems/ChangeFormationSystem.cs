using Towers.Components;
using Unity.Burst;
using Unity.Entities;

namespace Towers.Systems {
    /// <summary>
    /// This system triggers a change in formation every X seconds for all the towers
    /// </summary>
    public partial struct ChangeFormationSystem : ISystem {
        private static readonly float ChangeFormationRate = 3;
        private float _nextFormationChange;

        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            _nextFormationChange = ChangeFormationRate;
            state.RequireForUpdate<TowerComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            if (SystemAPI.Time.ElapsedTime < _nextFormationChange) return;
            _nextFormationChange += ChangeFormationRate;

            //TODO SCHEDULE THIS
            foreach (var tower in
                     SystemAPI.Query<RefRW<TowerComponent>>()
                         .WithNone<SpawnUnitsTag>()) {
                var currentFormation = tower.ValueRO.Formation;
                var newFormation = (Formation)((((int)currentFormation) + 1) % 3);
                //Debug.Log($"[ChangeFormationSystem] Changing Formation from [{(int)currentFormation}] -> [{(int)newFormation}]");
                tower.ValueRW.Formation = newFormation;
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }
}