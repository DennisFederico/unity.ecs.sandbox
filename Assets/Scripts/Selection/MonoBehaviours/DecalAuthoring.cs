using Selection.Components;
using Unity.Entities;
using UnityEngine;

namespace Selection.MonoBehaviours {
    public class DecalAuthoring : MonoBehaviour {
        private class DecalAuthoringBaker : Baker<DecalAuthoring> {
            public override void Bake(DecalAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new DecalComponentTag());
            }
        }
    }
}