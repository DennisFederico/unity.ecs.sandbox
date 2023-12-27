using Unity.Entities;
using Unity.Physics;

namespace Selection.Components {
    public struct RayCastBufferComponent : IBufferElementData {
        public RaycastInput Value;
    }
}