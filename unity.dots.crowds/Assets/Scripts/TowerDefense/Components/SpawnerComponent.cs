using Unity.Entities;

namespace TowerDefense.Components {
    public struct SpawnerComponent : IComponentData {
        public Entity Prefab;
        public float SpawnInterval;
        public float SpawnTimer;
    }
}