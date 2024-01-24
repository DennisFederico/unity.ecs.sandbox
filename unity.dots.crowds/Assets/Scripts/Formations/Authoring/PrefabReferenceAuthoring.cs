using Formations.Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Formations.Authoring {
    
    [AddComponentMenu("Prefab Reference")]
    public class PrefabReferenceAuthoring : MonoBehaviour {

        [SerializeField] private GameObject prefab;
        [SerializeField] private Color color;
        private class GetPrefabAuthoringBaker : Baker<PrefabReferenceAuthoring> {
            public override void Bake(PrefabReferenceAuthoring referenceAuthoring) {
                var entityPrefab = GetEntity(referenceAuthoring.prefab, TransformUsageFlags.Dynamic);
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new EntityPrefabComponent {
                    Value = entityPrefab,
                    Color = new float4(referenceAuthoring.color.r, referenceAuthoring.color.g, referenceAuthoring.color.b, referenceAuthoring.color.a)
                });
            }
        }
    }
}