using TowerDefenseBase.Components;
using TowerDefenseBase.Scriptables;
using Unity.Entities;
using UnityEngine;

namespace TowerDefenseBase.Mono {
    
    /// <summary>
    /// This combines individual authoring into one and add the EnemyTag component to the entity.
    /// </summary>
    public class EnemyAuthoring : MonoBehaviour {
        
        //Most of these should be added as a ScriptableObject and referenced here.
        [SerializeField] private EnemyDataSO enemyData;
        
        private class EnemyAuthoringBaker : Baker<EnemyAuthoring> {
            
            public override void Bake(EnemyAuthoring authoring) {
                DependsOn(authoring.enemyData);
                if (authoring.enemyData == null) {
                    Debug.Log("Cannot Bake enemy. EnemyDataSO is null.");
                    return;
                }
                
                var enemyComponentsSet = new ComponentTypeSet(
                    typeof(EnemyTag),
                    typeof(HealthComponent),
                    typeof(MoveSpeedComponent)
                    //typeof(VisualGameObjectComponent) //TODO SHOULD WE ADD THIS TO ANOTHER ENTITY AND REFERENCE IT HERE? BECAUSE THIS IS A MANAGED COMPONENT
                );

                var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
                AddComponent(entity, enemyComponentsSet);
                SetComponent(entity, new MoveSpeedComponent {Value = authoring.enemyData.speed});
                SetComponent(entity, new HealthComponent {Value = authoring.enemyData.health});
                AddComponentObject(entity, new VisualGameObjectComponent { VisualPrefab = authoring.enemyData.visualPrefab });
                // var additionalEntity = CreateAdditionalEntity(TransformUsageFlags.None, false, "VisualComponentEntity");
                // AddComponentObject(additionalEntity, new VisualGameObjectComponent {VisualPrefab = authoring.visualPrefab});
            }
        }
    }
}