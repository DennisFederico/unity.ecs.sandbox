using System.Collections.Generic;
using TowerDefenseBase.Components;
using TowerDefenseBase.Scriptables;
using Unity.Entities;
using UnityEngine;

namespace TowerDefenseBase.Mono {
    public class BuildingRegistryAuthoring : MonoBehaviour {
        [SerializeField] private List<TurretDataSO> turrets;
        
        private class TowerRegisterAuthoringBaker : Baker<BuildingRegistryAuthoring> {
            public override void Bake(BuildingRegistryAuthoring authoring) {
                foreach (var turret in authoring.turrets) {
                    DependsOn(turret);
                }
                
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent<BuildingRegistryTag>(entity);
                var towers = AddBuffer<BuildingsBufferElementData>(entity);
                var ghosts = AddBuffer<BuildingGhostsBufferElementData>(entity);
                var data = AddBuffer<BuildingDataBufferElementData>(entity);
                foreach (var turretSO in authoring.turrets) {
                    towers.Add(new BuildingsBufferElementData() {
                        Prefab = GetEntity(turretSO.turretPrefab, TransformUsageFlags.Dynamic)
                    });
                    ghosts.Add(new BuildingGhostsBufferElementData() {
                        Prefab = GetEntity(turretSO.ghostPrefab, TransformUsageFlags.Dynamic)
                    });
                    data.Add(new BuildingDataBufferElementData() {
                        Range = turretSO.fovRange,
                        FovAngle = turretSO.fovAngle
                    });
                }
            }
        }
    }
}