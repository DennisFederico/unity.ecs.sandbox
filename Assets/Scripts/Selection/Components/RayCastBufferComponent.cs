using Unity.Entities;
using Unity.Physics;

namespace Selection.Components {
    
    [InternalBufferCapacity(4)]
    public struct RayCastBufferComponent : IBufferElementData {
        public RaycastInput Value;
        public bool Additive;
    }
}