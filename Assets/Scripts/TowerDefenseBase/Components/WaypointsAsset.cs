using Unity.Entities;
using Unity.Mathematics;

namespace TowerDefenseBase.Components {
    
    public struct Waypoints {
        public BlobArray<float3> Points;
    }
    
    public struct WaypointsAsset : IComponentData {
        public BlobAssetReference<Waypoints> Waypoints;
    }
}