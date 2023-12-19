using TowerDefense.Components;
using Unity.Entities;
using UnityEngine;

namespace TowerDefense.MonoBehaviours {
    public class ImpactVfxAuthoring : MonoBehaviour {

        [SerializeField] private GameObject vfxPrefab;
        private class ImpactVfxAuthoringBaker : Baker<ImpactVfxAuthoring> {
            public override void Bake(ImpactVfxAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new ProjectileImpactComponent {
                    HitsLeft = 1,
                    VfxPrefab = GetEntity(authoring.vfxPrefab, TransformUsageFlags.Dynamic)
                });
            }
        }
    }
}