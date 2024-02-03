using Unity.Entities;

namespace Switching.Components {
    public struct PrefabHolderComponent : IComponentData {
        public Entity Prefab;
    }
}