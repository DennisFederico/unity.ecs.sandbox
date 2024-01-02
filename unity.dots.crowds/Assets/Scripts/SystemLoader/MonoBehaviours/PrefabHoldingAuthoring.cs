using SystemLoader.Components;
using Unity.Entities;
using UnityEngine;

namespace SystemLoader.MonoBehaviours {
    public class PrefabHoldingAuthoring : MonoBehaviour {
        
        [SerializeField] private GameObject prefab;
        private class PrefabHolderAuthoringBaker : Baker<PrefabHoldingAuthoring> {
            public override void Bake(PrefabHoldingAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.None);
                var prefabEntity = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic);
                AddComponent(entity, new PrefabHoldingComponent() {
                    Prefab = prefabEntity
                });
            }
        }
    }
}