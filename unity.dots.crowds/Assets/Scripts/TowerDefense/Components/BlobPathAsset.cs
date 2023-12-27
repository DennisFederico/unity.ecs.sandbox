using Unity.Entities;
using Unity.Mathematics;

namespace TowerDefense.Components {
    
    public struct BlobPath : IComponentData {
        public BlobArray<float3> Waypoints;
    }
    
    public struct BlobPathAsset : IComponentData {
        public BlobAssetReference<BlobPath> Path;
    }
}