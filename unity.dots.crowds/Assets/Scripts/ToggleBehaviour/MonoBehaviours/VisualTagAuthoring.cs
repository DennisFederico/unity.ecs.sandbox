using ToggleBehaviour.Components;
using Unity.Entities;
using UnityEngine;

namespace ToggleBehaviour.MonoBehaviours {
    public class VisualTagAuthoring : MonoBehaviour {
        private class VisualTagAuthoringBaker : Baker<VisualTagAuthoring> {
            public override void Bake(VisualTagAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<VisualComponentTag>(entity);
            }
        }
    }
}