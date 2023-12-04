using Unity.Entities;
using Unity.Mathematics;

namespace Spawner.Components {
    public struct SpawnComponentData : IComponentData {
        public Entity Prefab;
        public float SpawnRate;
        public float NextSpawnTime;
        public Random InternalRandom;
        public int SpawnCount;
    }
}