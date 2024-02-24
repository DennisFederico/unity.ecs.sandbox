using Unity.Entities;
using Unity.Physics;

namespace TowerDefenseEcs.Components {
    
    [InternalBufferCapacity(3)]
    public struct DestroyBuildingRayInputData : IBufferElementData {
        public RaycastInput Value;
    }
}