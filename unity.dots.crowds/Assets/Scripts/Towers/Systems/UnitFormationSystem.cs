using Towers.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace Towers.Systems {
    //Using a cached target position and some control that it has been reached
    //THIS IS A MOVE SYSTEM THAT KEEPS UNITS IN FORMATION
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial struct UnitFormationSystem : ISystem {
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<MoveComponent>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            //This should be changed to query and parallelize by tower, avoiding each unit having to check its parent
            foreach (var (formationData,
                         moveData,
                         parentReference) in
                     SystemAPI.Query<
                         RefRW<FormationComponent>,
                         RefRW<MoveComponent>,
                         ParentEntityReferenceComponent>()) {
                
                TowerComponent parentTowerData = state.EntityManager.GetComponentData<TowerComponent>(parentReference.ParentEntity);
                var parentFormation = parentTowerData.Formation;
                var currentFormation = formationData.ValueRO.Value;

                if (currentFormation != parentFormation) {
                    formationData.ValueRW.Value = parentFormation;
                    int unitFormationIndex = formationData.ValueRO.Index;
                    var targetPosition = parentTowerData.Formation.Position(parentTowerData.Radius, unitFormationIndex / (float)parentTowerData.UnitCount);
                    LocalTransform parentTransform = state.EntityManager.GetComponentData<LocalTransform>(parentReference.ParentEntity);
                    targetPosition = parentTransform.TransformPoint(targetPosition);
                    moveData.ValueRW.TargetPosition = targetPosition;
                    //Enable the move component
                    //enabled.ValueRW = true;
                }
            }
        }
    }
}