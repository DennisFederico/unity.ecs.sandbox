using Unity.Entities;
using Unity.Physics.Authoring;

namespace TowerDefenseEcs.Components {
    public struct BuildingSystemConfigData : IComponentData {
        public PhysicsCategoryTags InputSystemTag;
        public PhysicsCategoryTags TerrainTag;
        public PhysicsCategoryTags TurretTag;
        public PhysicsCategoryTags PlacingObstacles;
    }
}