using Unity.Entities;

namespace TowerDefenseBase.Components {
    public struct ProjectileImpactComponent : IComponentData {
        public Entity VfxPrefab;
        public int HitsLeft;
    }
    
    // Hack to avoid hitting the same target more than once
    public struct Hits : IBufferElementData {
        public Entity Entity;
    }
}