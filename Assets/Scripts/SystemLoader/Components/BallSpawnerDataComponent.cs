using Unity.Entities;
using Unity.Mathematics;

namespace SystemLoader.Components {
    
    public struct BallSpawnerDataComponent : IComponentData {
        public float3 SpawnCenter;
        public float3 SpawnRange;
        public int SpawnCount;
        public float SpawnPerSecond;
        public int SpawnedCount;
    }
}