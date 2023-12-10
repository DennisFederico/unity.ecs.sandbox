using Unity.Entities;

namespace Towers.Components {
    public struct ParentEntityReferenceComponent : ISharedComponentData {
        public Entity ParentEntity;
    }
}