using Unity.Entities;

namespace Formations.Components {
    public struct ParentEntityReferenceComponent : ISharedComponentData {
        public Entity ParentEntity;
    }
}