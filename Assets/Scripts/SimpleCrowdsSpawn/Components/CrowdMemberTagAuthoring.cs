using Unity.Entities;
using UnityEngine;

namespace SimpleCrowdsSpawn.Components {
    
    public struct CrowdMemberTag : IComponentData {
    }
    
    public class CrowdMemberTagAuthoring : MonoBehaviour {
        private class CrowdMemberTagAuthoringBaker : Baker<CrowdMemberTagAuthoring> {
            public override void Bake(CrowdMemberTagAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new CrowdMemberTag());
            }
        }
    }
}