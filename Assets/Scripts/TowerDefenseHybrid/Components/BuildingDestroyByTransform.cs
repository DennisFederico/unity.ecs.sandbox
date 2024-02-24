using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Authoring;

namespace TowerDefenseHybrid.Components {
    
    [InternalBufferCapacity(3)]
    public struct BuildingDestroyByTransform : IBufferElementData {
        //public sbyte buildingId; //TODO Should we have it to confirm at least the type of building?
        public float3 Position;
        public PhysicsCategoryTags ShapeTag;
    }
}