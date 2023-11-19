using Unity.Entities;
using UnityEngine;

namespace Crowds.Components {
    public class CrowdSpawnerAuthoring : MonoBehaviour {
        
        [SerializeField] private GameObject prefab;
        
        private class CrowdSpawnerAuthoringBaker : Baker<CrowdSpawnerAuthoring> {
            public override void Bake(CrowdSpawnerAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new CrowdSpawner { Prefab = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic) });
            }
        }
    }
}