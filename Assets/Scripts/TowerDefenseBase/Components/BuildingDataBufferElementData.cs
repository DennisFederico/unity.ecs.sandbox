using Unity.Entities;

namespace TowerDefenseBase.Components {
    
    [InternalBufferCapacity(2)]
    public struct BuildingDataBufferElementData : IBufferElementData {
        public float Range;
        public float FovAngle;
    }
}