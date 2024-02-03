using Unity.Entities;
using Unity.Mathematics;

namespace AStar.Components {
    
    [InternalBufferCapacity(32)]
    public struct PathPositionElement : IBufferElementData {
        public float3 Position;
    }
}