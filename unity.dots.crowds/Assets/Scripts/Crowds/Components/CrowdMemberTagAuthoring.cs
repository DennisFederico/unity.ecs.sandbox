using Unity.Entities;
using UnityEngine;

namespace Crowds.Components {
    public class CrowdMemberTagAuthoring : MonoBehaviour {
        private class CrowdMemberTagAuthoringBaker : Baker<CrowdMemberTagAuthoring> {
            public override void Bake(CrowdMemberTagAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new CrowdMemberTag());
            }
        }
    }
}