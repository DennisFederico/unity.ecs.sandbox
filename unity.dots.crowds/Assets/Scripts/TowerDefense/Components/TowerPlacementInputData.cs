using Unity.Entities;
using Unity.Physics;

namespace TowerDefense.Components {
    public struct TowerPlacementInputData : IBufferElementData {
        public RaycastInput Value;
        public int TowerIndex;
    }
}