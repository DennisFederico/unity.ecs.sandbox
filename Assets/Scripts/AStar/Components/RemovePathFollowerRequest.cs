using Unity.Entities;

namespace AStar.Components {
    
    [InternalBufferCapacity(5)]
    public struct RemovePathFollowerRequest : IBufferElementData {
        public int Value;
    }
}