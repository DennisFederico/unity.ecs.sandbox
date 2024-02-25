using Unity.Entities;
using Unity.Physics;

namespace TowerDefenseEcs.Components {
    
    [InternalBufferCapacity(3)]
    public struct DestroyBuildingData : IBufferElementData {
        public RaycastInput Value;
    }
}