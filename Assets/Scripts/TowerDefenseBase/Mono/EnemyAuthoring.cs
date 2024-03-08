using TowerDefenseBase.Components;
using Unity.Entities;
using UnityEngine;

namespace TowerDefenseBase.Mono {
    
    /// <summary>
    /// This combines individual authoring into one and add the EnemyTag component to the entity.
    /// </summary>
    public class EnemyAuthoring : MonoBehaviour {
        
        //Most of these should be added as a ScriptableObject and referenced here.
        [SerializeField] private float speed = 3f;
        [SerializeField] private GameObject visualPrefab;
        [SerializeField] private float health;
        
        private class EnemyAuthoringBaker : Baker<EnemyAuthoring> {
            
            public override void Bake(EnemyAuthoring authoring) {
                var enemyComponentsSet = new ComponentTypeSet(
                    typeof(EnemyTag),
                    typeof(HealthComponent),
                    typeof(MoveSpeedComponent)
                    //typeof(VisualGameObjectComponent) //TODO SHOULD WE ADD THIS TO ANOTHER ENTITY AND REFERENCE IT HERE? BECAUSE THIS IS A MANAGED COMPONENT
                );

                var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
                AddComponent(entity, enemyComponentsSet);
                SetComponent(entity, new MoveSpeedComponent {Value = authoring.speed});
                SetComponent(entity, new HealthComponent {Value = authoring.health});
                AddComponentObject(entity, new VisualGameObjectComponent { VisualPrefab = authoring.visualPrefab });
                // var additionalEntity = CreateAdditionalEntity(TransformUsageFlags.None, false, "VisualComponentEntity");
                // AddComponentObject(additionalEntity, new VisualGameObjectComponent {VisualPrefab = authoring.visualPrefab});
            }
        }
    }
}