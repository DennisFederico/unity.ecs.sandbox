using TowerDefenseEcs.Components;
using Unity.Entities;
using Unity.Physics.Authoring;
using UnityEngine;

namespace TowerDefenseEcs.Mono {
    public class BuildingSystemConfigDataAuthoring : MonoBehaviour {
        public PhysicsCategoryTags InputSystemTag;
        public PhysicsCategoryTags TerrainTag;
        public PhysicsCategoryTags TurretTag;
        public PhysicsCategoryTags PlacingObstacles;

        public class BuildingSystemConfigDataBaker : Baker<BuildingSystemConfigDataAuthoring> {
            public override void Bake(BuildingSystemConfigDataAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity,
                    new BuildingSystemConfigData {
                        InputSystemTag = authoring.InputSystemTag,
                        TerrainTag = authoring.TerrainTag,
                        TurretTag = authoring.TurretTag,
                        PlacingObstacles = authoring.PlacingObstacles
                    });
            }
        }
    }
}