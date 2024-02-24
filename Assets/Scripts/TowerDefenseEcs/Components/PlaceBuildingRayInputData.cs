using Unity.Entities;
using Unity.Physics;

namespace TowerDefenseEcs.Components {
    
    [InternalBufferCapacity(3)]
    public struct PlaceBuildingRayInputData : IBufferElementData {
        public RaycastInput Value;
        public int TowerIndex;
    }
}