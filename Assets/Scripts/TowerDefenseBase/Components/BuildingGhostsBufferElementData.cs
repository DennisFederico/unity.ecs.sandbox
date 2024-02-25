using Unity.Entities;

namespace TowerDefenseBase.Components {
    
    [InternalBufferCapacity(2)]
    public struct BuildingGhostsBufferElementData : IBufferElementData {
        public Entity Prefab;
    }
}