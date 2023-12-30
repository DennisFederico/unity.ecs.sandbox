using Unity.Entities;

namespace ToggleBehaviour.Components {
    public struct PrefabHolderComponent : IComponentData {
        public Entity Prefab;
    }
}