using TowerDefenseBase.Components;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

namespace TowerDefenseBase.Systems {
    
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(TurretAttackJobBasedSystem))]
    public partial struct UpdateFireRateTimerSystem : ISystem {
        
        /// <summary>
        /// Just updates / resets the timer before the next frame starts
        /// </summary>
        /// <param name="state"></param>
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<FireRateComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var elapsedDeltaTime = SystemAPI.Time.DeltaTime;
            foreach (var timer in SystemAPI.Query<RefRW<FireRateComponent>>()) {
                if (timer.ValueRO.HasElapsed()) {
                    timer.ValueRW.ResetTimer();
                } else {
                    timer.ValueRW.ElapseTime(elapsedDeltaTime);
                }
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {

        }
    }
}