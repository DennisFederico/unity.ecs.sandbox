using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Authoring;

namespace Selection.Components {
    [InternalBufferCapacity(2)]
    public struct SelectionVerticesBufferComponent : IBufferElementData {
        public NativeArray<float3> Vertices;
        public bool Additive;
        public PhysicsCategoryTags belongsTo;
        public PhysicsCategoryTags collidesWith;
    }
}