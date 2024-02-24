using Unity.Entities;

namespace TowerDefenseBase.Components {
    public struct TurretDataComponent : IComponentData {
        public Entity ProjectilePrefab;
        public float ShootTimer;
    }
}