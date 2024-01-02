using Unity.Entities;

namespace SystemLoader.Components {
    public struct PrefabHoldingComponent : IComponentData {
        public Entity Prefab;
    }
}