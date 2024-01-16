using Unity.Entities;

namespace AStar.Components {
    public struct PrefabHoldingSingleton : IComponentData {
        public Entity Prefab;
    }
}