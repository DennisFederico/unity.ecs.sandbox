using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Authoring;

namespace Selection.Components {
    [InternalBufferCapacity(2)]
    public struct SelectionBoxBufferComponent : IBufferElementData {
        public float3 BoxCenter;
        public float3 BoxSize;
        public quaternion BoxOrientation;
        public bool Additive;
        public PhysicsCategoryTags BelongsTo;
        public PhysicsCategoryTags CollidesWith;
    }
}