using Unity.Entities;

namespace Collider.Components {
    public struct ImpactVfxComponent : IComponentData {
        public Entity VfxPrefab;
    }
}