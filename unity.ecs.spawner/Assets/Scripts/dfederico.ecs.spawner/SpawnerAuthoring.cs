using Unity.Entities;
using UnityEngine;

namespace dfederico.ecs.spawner {
    public class SpawnerAuthoring : MonoBehaviour {

        [SerializeField] private GameObject prefab;
        [SerializeField] private float spawnRate;
        
        private class SpawnerAuthoringBaker : Baker<SpawnerAuthoring> {
            public override void Bake(SpawnerAuthoring authoring) {
                var transform = GetComponent<Transform>();
                // May not be required in this case since Transform is always present in a GameObject
                if (transform == null) {
                    Debug.LogError("SpawnerAuthoring requires a Transform component");
                    return;
                }
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Spawner() {
                    Prefab = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic),
                    SpawnerCenter = transform.position,
                    NextSpawnTime = 0.0f,
                    SpawnRate = authoring.spawnRate
                });
            }
        }
    }
}