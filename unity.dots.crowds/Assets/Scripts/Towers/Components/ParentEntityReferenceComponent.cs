using Unity.Entities;

namespace Towers.Components {
    public struct ParentEntityReferenceComponent : IComponentData {
        public Entity ParentEntity;
    }
}