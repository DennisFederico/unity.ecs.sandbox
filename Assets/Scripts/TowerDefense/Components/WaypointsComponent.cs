using Unity.Entities;
using Unity.Mathematics;

namespace TowerDefense.Components {
    
    [InternalBufferCapacity(4)]
    public struct WaypointsComponent : IBufferElementData {
        public float3 Value;
    }
    
}