using Unity.Entities;
using UnityEngine;

namespace sandbox {
    
    public class SpawnerComponentAuthoring : MonoBehaviour {
        public GameObject prefab;

        public class SpawnerComponentBaker : Baker<SpawnerComponentAuthoring> {
            public override void Bake(SpawnerComponentAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new SpawnerComponent { Prefab = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic) });
            }
        }
    }
    
    public struct SpawnerComponent : IComponentData {
        public Entity Prefab;
    }
}