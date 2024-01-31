using Unity.Entities;
using Unity.Mathematics;

namespace SimpleCrowdsSpawn.Components {
    
    [InternalBufferCapacity(3)]
    public struct PlaceSpawnerRequestBuffer : IBufferElementData {
        public bool SelectRandom;
        public float3 Position;
        public quaternion Rotation;
    }
}