using Switching.Components;
using Unity.Entities;
using UnityEngine;

namespace Switching.MonoBehaviours {
    public class PrefabHolderAuthoring : MonoBehaviour {
        
        [SerializeField] private GameObject prefab;
        private class PrefabHolderAuthoringBaker : Baker<PrefabHolderAuthoring> {
            public override void Bake(PrefabHolderAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.None);
                var prefabEntity = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic);
                AddComponent(entity, new PrefabHolderComponent() {
                    Prefab = prefabEntity
                });
            }
        }
    }
}