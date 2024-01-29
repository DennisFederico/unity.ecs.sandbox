using Unity.Entities;
using UnityEngine;

namespace Collider.Components {
    
    [InternalBufferCapacity(4)]
    public struct SpawnRequestComponentBuffer : IBufferElementData {
        public int MouseButton;
        public Ray Ray;
        public float Distance;
    }
}