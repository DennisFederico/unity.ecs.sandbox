using System;
using Unity.Entities;

namespace Formations.Components {
    public struct ParentEntityReferenceComponent : ISharedComponentData, IEquatable<ParentEntityReferenceComponent> {
        public Entity ParentEntity;

        public bool Equals(ParentEntityReferenceComponent other) {
            return ParentEntity.Equals(other.ParentEntity);
        }

        public override int GetHashCode() {
            return ParentEntity.GetHashCode();
        }
    }
}