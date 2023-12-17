using Unity.Entities;

namespace TowerDefense.Components {
    public struct SpawnerDataComponent : IComponentData {
        public Entity Prefab;
        public float SpawnInterval;
        public float SpawnTimer;
    }
}