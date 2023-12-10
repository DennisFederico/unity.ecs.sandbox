using Towers.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Towers.Systems {
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    //Not sure if the properties is needed since all the queries are
    //already as RequireForUpdate during creation
    [RequireMatchingQueriesForUpdate]
    public partial struct SpawnUnitsSystem : ISystem {
        private EntityQuery _spawnUnitsQuery;

        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<SpawnUnitsTag>(); //A Query would include other components
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);
            
            foreach (var (transform, 
                         tower, 
                         prefab, 
                         enabled, 
                         entity) in 
                     SystemAPI.Query<
                         RefRO<LocalTransform>,
                         RefRO<TowerComponent>, 
                         RefRO<EntityPrefabComponent>, 
                         EnabledRefRW<SpawnUnitsTag>
                     >().WithEntityAccess()) {
                
                //SPAWN THE UNITS
                var formation = tower.ValueRO.Formation;

                var sharedParent = new ParentEntityReferenceComponent {
                    ParentEntity = entity
                };

                for (int index = 0; index < tower.ValueRO.UnitCount; index++) {
                    var unitEntity = ecb.Instantiate(prefab.ValueRO.Value);
                    float fraction = index / (float) tower.ValueRO.UnitCount;
                    var relativePosition = formation.Position(tower.ValueRO.Radius, fraction);
                    // ReSharper disable once PossiblyImpureMethodCallOnReadonlyVariable
                    var worldPosition = transform.ValueRO.TransformPoint(relativePosition);
                    
                    // ecb.AddSharedComponent(unitEntity, parentEntityReferenceComponent);
                    ecb.AddSharedComponent(unitEntity, sharedParent);
                    
                    ecb.AddComponent(unitEntity, new FormationComponent {
                        Value = formation,
                        Index = index
                    });
                    
                    ecb.AddComponent(unitEntity, new MoveComponent {
                        TargetPosition = worldPosition,
                        Speed = 30
                    });
                    
                    // SHOULD WE SET THE POSITION
                     ecb.AddComponent(unitEntity, new LocalTransform {
                         Position = transform.ValueRO.Position,
                         Rotation = quaternion.identity,
                         Scale = 1f
                     });
                }
                
                //REMOVE TO AVOID GOING BACK IN THIS SYSTEM - DISABLE STILL TRIGGERS THE UPDATE
                enabled.ValueRW = false;
                ecb.RemoveComponent<SpawnUnitsTag>(entity);
            }
        } 

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }
}