using Unity.Entities;
using UnityEngine;

namespace Recap101.Components {
    public class VisualGOAuthoring : MonoBehaviour {
        
        [SerializeField] private GameObject visualPrefab;
        private class SkeletonAuthoringBaker : Baker<VisualGOAuthoring> {
            public override void Bake(VisualGOAuthoring authoring) {
                var visualComponent = new VisualGameObjectComponent {
                    VisualPrefab = authoring.visualPrefab
                };
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponentObject(entity, visualComponent);
            }
        }
    }
}