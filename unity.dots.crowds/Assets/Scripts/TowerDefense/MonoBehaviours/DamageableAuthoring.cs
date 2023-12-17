using TowerDefense.Components;
using Unity.Entities;
using UnityEngine;

namespace TowerDefense.MonoBehaviours {
    public class DamageableAuthoring : MonoBehaviour {

        [SerializeField] private float health;
        private class DamageableAuthoringBaker : Baker<DamageableAuthoring> {
            public override void Bake(DamageableAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new HealthComponent {
                    Value = authoring.health
                });
            }
        }
    }
}