using Unity.Entities;

namespace Recap101.Components {
    public struct SpawnerComponent : IComponentData {
        public Entity Prefab;
        public float SpawnInterval;
        public float SpawnTimer;
    }
}