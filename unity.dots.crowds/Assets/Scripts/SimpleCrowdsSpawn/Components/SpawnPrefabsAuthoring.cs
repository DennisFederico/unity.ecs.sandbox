using Unity.Entities;
using UnityEngine;

namespace SimpleCrowdsSpawn.Components {

    public struct CrowdSpawnerPrefabs : IComponentData {
        public Entity SpawnerMarker;
        public Entity Prefab; //Might not be needed, yet!
    }

    public struct CrowdSpawner : IComponentData {
        public Entity Spawner; //Track the current spawner entity
        public Entity Prefab;
    }

    public class SpawnPrefabsAuthoring : MonoBehaviour {

        [SerializeField] private GameObject staticSpawnerVisual;
        [SerializeField] private GameObject spawnPrefab;

        public class SpawnMakerBaker : Baker<SpawnPrefabsAuthoring> {
            public override void Bake(SpawnPrefabsAuthoring authoring) {
                var prefabsHolder = GetEntity(TransformUsageFlags.None);

                var spawnPrefab = GetEntity(authoring.spawnPrefab, TransformUsageFlags.Dynamic);

                AddComponent(prefabsHolder, new CrowdSpawnerPrefabs {
                    SpawnerMarker = GetEntity(authoring.staticSpawnerVisual, TransformUsageFlags.Dynamic),
                    Prefab = spawnPrefab
                });

                var spawner = CreateAdditionalEntity(TransformUsageFlags.None, false, "SpawnerReference");
                AddComponent(spawner, new CrowdSpawner {
                    Spawner = Entity.Null,
                    Prefab = spawnPrefab
                });
            }
        }
    }
}