using Unity.Entities;

namespace TowerDefenseBase.Components {
    public struct SpawnerDataComponent : IComponentData {
        public Entity Prefab;
        public float SpawnInterval;
        public float SpawnTimer;
    }
}