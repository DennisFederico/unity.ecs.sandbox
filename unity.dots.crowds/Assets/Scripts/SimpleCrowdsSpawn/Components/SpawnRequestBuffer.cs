using Unity.Entities;

namespace SimpleCrowdsSpawn.Components {
    
    [InternalBufferCapacity(2)]
    public struct SpawnRequestBuffer : IBufferElementData {
        public int Amount;
    }
}