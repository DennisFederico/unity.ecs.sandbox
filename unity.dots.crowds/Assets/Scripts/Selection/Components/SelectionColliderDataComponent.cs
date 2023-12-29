using Unity.Entities;
using Unity.Physics.Authoring;

namespace Selection.Components {
    public struct SelectionColliderDataComponent : IComponentData {
        public bool Additive;
        public PhysicsCategoryTags BelongsTo;
        public PhysicsCategoryTags CollidesWith;
    }
}