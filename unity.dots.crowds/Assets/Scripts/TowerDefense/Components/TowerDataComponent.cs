using Unity.Entities;
using Unity.Physics;

namespace TowerDefense.Components {
    public struct TowerDataComponent : IComponentData {
        public Entity ProjectilePrefab;
        public float ShootTimer;
        public float ShootFrequency;
        public CollisionFilter Filter;
        public float Range;
    }
}