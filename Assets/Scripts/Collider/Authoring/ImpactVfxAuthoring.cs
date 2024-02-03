using Collider.Components;
using Unity.Entities;
using UnityEngine;

namespace Collider.Authoring {
    public class ImpactVfxAuthoring : MonoBehaviour {

        [SerializeField] private GameObject vfxPrefab;
        
        private class ImpactVfxAuthoringBaker : Baker<ImpactVfxAuthoring> {
            public override void Bake(ImpactVfxAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new ImpactVfxComponent {
                    VfxPrefab = GetEntity(authoring.vfxPrefab, TransformUsageFlags.Dynamic)
                });
                AddComponent(entity, new TimeToLiveComponent {
                    CreatedAt = Time.time,
                    TimeToLive = 3f
                });
            }
        }
    }
}