using Unity.Entities;
using Unity.Physics;

namespace TowerDefense.Components {
    
    public struct TowerConfig : IComponentData {
        public float ShootFrequency;
        public CollisionFilter Filter;
        public float Range;
    }
    
    public struct TowerConfigAsset : IComponentData {
        public BlobAssetReference<TowerConfig> Config;
    }
}