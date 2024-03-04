using Unity.Entities;
using Unity.Mathematics;

namespace TowerDefenseHybrid.Components {
    
    [InternalBufferCapacity(3)]
    public struct BuildingPlacementByTransform : IBufferElementData {
        public sbyte BuildingId;
        public float3 Position;
        public quaternion Rotation;
    }
}