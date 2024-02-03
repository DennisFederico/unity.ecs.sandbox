using Unity.Entities;
using UnityEngine;

namespace Recap101.Components {
    public class VisualGameObjectAuthoring : MonoBehaviour {
        [SerializeField] private GameObject visualPrefab;

        private class SkeletonAuthoringBaker : Baker<VisualGameObjectAuthoring> {
            public override void Bake(VisualGameObjectAuthoring authoring) {
                var visualComponent = new VisualGameObjectComponent {
                    VisualPrefab = authoring.visualPrefab
                };
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponentObject(entity, visualComponent);
            }
        }
    }
}