using Unity.Entities;
using Unity.Physics;

namespace TowerDefenseBase.Components {
    
    public struct TurretConfig : IComponentData {
        public CollisionFilter Filter;
        public float Range;
        public float FovAngle;
    }
    
    public struct TurretConfigAsset : IComponentData {
        public BlobAssetReference<TurretConfig> Config;
    }
}