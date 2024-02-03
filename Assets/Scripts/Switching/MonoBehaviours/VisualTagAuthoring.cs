using Switching.Components;
using Unity.Entities;
using UnityEngine;

namespace Switching.MonoBehaviours {
    public class VisualTagAuthoring : MonoBehaviour {
        private class VisualTagAuthoringBaker : Baker<VisualTagAuthoring> {
            public override void Bake(VisualTagAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<VisualComponentTag>(entity);
            }
        }
    }
}