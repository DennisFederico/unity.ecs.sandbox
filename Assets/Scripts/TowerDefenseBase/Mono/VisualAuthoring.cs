using TowerDefenseBase.Components;
using Unity.Entities;
using UnityEngine;

namespace TowerDefenseBase.Mono {
    public class VisualAuthoring : MonoBehaviour {
        [SerializeField] private GameObject visualPrefab;

        private class VisualAuthoringBaker : Baker<VisualAuthoring> {
            public override void Bake(VisualAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponentObject(entity, new VisualGameObjectComponent {
                    VisualPrefab = authoring.visualPrefab
                });
            }
        }
    }
}