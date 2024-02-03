using Unity.Entities;
using Unity.Mathematics;

namespace AStar.Components {
    
    [InternalBufferCapacity(500)]
    public struct CreateNewPathFollowerRequest : IBufferElementData {
        public float3 StartPosition;
        public float3 EndPosition;
        public float TimeToLive;
    }
}