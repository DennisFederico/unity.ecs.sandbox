using Unity.Entities;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace SwarmSpawner.Components {
    
    [RequireComponent(typeof(AreaAuthoring))]
    public class SpawnAuthoring : MonoBehaviour {
        
        public GameObject prefab;
        public float spawnRate;
        public int spawnCount;

        private class SpawnAuthoringBaker : Baker<SpawnAuthoring> {
            public override void Bake(SpawnAuthoring authoring) {

                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new SpawnComponentData {
                    Prefab = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic),
                    SpawnRate = authoring.spawnRate,
                    InternalRandom = new Random((uint) new System.Random().Next()),
                    SpawnCount = authoring.spawnCount,
                    NextSpawnTime = 0
                });
            }
        }
    }
}