using Unity.Entities;

namespace TowerDefense.Components {
    public struct ProjectileImpactComponent : IComponentData {
        public Entity VfxPrefab;
        public int HitsLeft;
    }
    
    // Every projectile has a list of entities it has hit
    public struct Hits : IBufferElementData {
        public Entity Entity;
    }
}