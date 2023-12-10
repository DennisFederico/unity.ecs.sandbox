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
            foreach (var (unitData, parentReference, entity) in
                     SystemAPI.Query<RefRW<MoveComponent>,
                         ParentEntityReferenceComponent>().WithEntityAccess()) {
                
                TowerComponent parentTowerData = state.EntityManager.GetComponentData<TowerComponent>(parentReference.ParentEntity);
                var parentFormation = parentTowerData.Formation;
                var currentFormation = unitData.ValueRO.Formation;
                
                //Debug.Log($"Formation {entity.Index} to {parentFormation} from {currentFormation}");

                if (currentFormation != parentFormation) {
                    Debug.Log($"Formation Changed to {parentFormation} from {currentFormation}");
                    unitData.ValueRW.Formation = parentFormation;
                    int unitFormationIndex = unitData.ValueRO.FormationIndex;
                    var targetPosition = parentTowerData.Formation.Position(parentTowerData.Radius, unitFormationIndex / (float)parentTowerData.UnitCount);
                    LocalTransform parentTransform = state.EntityManager.GetComponentData<LocalTransform>(parentReference.ParentEntity);
                    targetPosition = parentTransform.TransformPoint(targetPosition);
                    unitData.ValueRW.TargetPosition = targetPosition;
                    //Enable the move component
                    //enabled.ValueRW = true;
                }
            }
        }
    }
}