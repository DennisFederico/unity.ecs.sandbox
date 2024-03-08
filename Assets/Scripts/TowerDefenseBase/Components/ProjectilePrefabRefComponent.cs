using Unity.Entities;

namespace TowerDefenseBase.Components {
    public struct ProjectilePrefabRefComponent : IComponentData {
        public Entity ProjectilePrefab;
    }
}