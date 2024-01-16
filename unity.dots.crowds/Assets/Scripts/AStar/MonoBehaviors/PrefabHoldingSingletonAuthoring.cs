using AStar.Components;
using Unity.Entities;
using UnityEngine;

namespace AStar.MonoBehaviors {

    public class PrefabHoldingSingletonAuthoring : MonoBehaviour {
        
        [SerializeField] private GameObject prefab;

        private class MovableUnitAuthoringBaker : Baker<PrefabHoldingSingletonAuthoring> {
            public override void Bake(PrefabHoldingSingletonAuthoring authoring) {
                var holder = GetEntity(TransformUsageFlags.None);
                AddComponent(holder, new PrefabHoldingSingleton {
                    Prefab = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic)
                });
            }
        }
    }
}