using Unity.Entities;
using Unity.Mathematics;

namespace dfederico.ecs.spawner {
    public struct Spawner : IComponentData {
        public Entity Prefab;
        public float3 SpawnerCenter;
        public float SpawnRadius;
        public float NextSpawnTime;
        public float SpawnRate;
    }
}