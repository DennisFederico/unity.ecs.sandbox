using Unity.Entities;

namespace TowerDefenseBase.Components {
    
    [InternalBufferCapacity(2)]
    public struct BuildingsBufferElementData : IBufferElementData {
        public Entity Prefab;
    }
}