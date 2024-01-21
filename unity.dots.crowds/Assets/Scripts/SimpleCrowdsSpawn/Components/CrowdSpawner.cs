using Unity.Entities;

namespace SimpleCrowdsSpawn.Components {
    public struct CrowdSpawner : IComponentData {
        public Entity Prefab;
    }
}