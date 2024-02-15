using TowerDefense.Components;
using Unity.Entities;
using UnityEngine;

namespace TowerDefense.MonoBehaviours {
    public class HealthAuthoring : MonoBehaviour {

        [SerializeField] private float health;
        private class HealthAuthoringBaker : Baker<HealthAuthoring> {
            public override void Bake(HealthAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new HealthComponent {
                    Value = authoring.health
                });
            }
        }
    }
}