using Unity.Entities;
using Unity.Mathematics;

namespace TowerDefense.Components {
    
    public struct WaypointsArray : IComponentData {
        public BlobArray<float3> Waypoints;
    }
    
    public struct WaypointsAsset : IComponentData {
        public BlobAssetReference<WaypointsArray> Path;
    }
}