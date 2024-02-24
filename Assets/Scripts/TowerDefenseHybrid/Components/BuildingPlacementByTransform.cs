using Unity.Entities;
using Unity.Mathematics;

namespace TowerDefenseHybrid.Components {
    
    [InternalBufferCapacity(3)]
    public struct BuildingPlacementByTransform : IBufferElementData {
        public sbyte buildingId; //TODO Enum?
        public float3 Position;
        public quaternion Rotation;
    }
}