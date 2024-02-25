using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Authoring;

namespace TowerDefenseEcs.Components {
    
    [InternalBufferCapacity(3)]
    public struct PlaceBuildingData : IBufferElementData {
        public RaycastInput RayInput;
        public quaternion Rotation;
        public int BuildingIndex;
        public PhysicsCategoryTags ObstacleLayers;
    }
}