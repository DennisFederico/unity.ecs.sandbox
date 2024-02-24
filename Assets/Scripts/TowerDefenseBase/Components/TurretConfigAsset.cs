using Unity.Entities;
using Unity.Physics;

namespace TowerDefenseBase.Components {
    
    public struct TurretConfig : IComponentData {
        public float ShootFrequency;
        public CollisionFilter Filter;
        public float Range;
    }
    
    public struct TurretConfigAsset : IComponentData {
        public BlobAssetReference<TurretConfig> Config;
    }
}