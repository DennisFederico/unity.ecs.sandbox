using Unity.Entities;

namespace TowerDefense.Components {
    public struct TowersBufferElementData : IBufferElementData {
        public Entity Prefab;
    }
}