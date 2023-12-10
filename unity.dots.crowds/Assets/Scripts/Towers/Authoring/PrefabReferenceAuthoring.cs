using Towers.Components;
using Unity.Entities;
using UnityEngine;

namespace Towers.Authoring {
    
    [AddComponentMenu("Prefab Reference")]
    public class PrefabReferenceAuthoring : MonoBehaviour {

        [SerializeField] private GameObject prefab;
        private class GetPrefabAuthoringBaker : Baker<PrefabReferenceAuthoring> {
            public override void Bake(PrefabReferenceAuthoring referenceAuthoring) {
                var entityPrefab = GetEntity(referenceAuthoring.prefab, TransformUsageFlags.Dynamic);
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new EntityPrefabComponent {
                    Value = entityPrefab
                });
            }
        }
    }
}