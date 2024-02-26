using System.Collections.Generic;
using TowerDefenseBase.Components;
using Unity.Entities;
using UnityEngine;

namespace TowerDefenseBase.Mono {
    public class BuildingRegistryAuthoring : MonoBehaviour {
        
        [SerializeField] private List<GameObject> buildings;
        [SerializeField] private List<GameObject> ghosts;
        private class TowerRegisterAuthoringBaker : Baker<BuildingRegistryAuthoring> {
            public override void Bake(BuildingRegistryAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent<BuildingRegistryTag>(entity);
                var towers = AddBuffer<BuildingsBufferElementData>(entity);
                foreach (var tower in authoring.buildings) {
                    towers.Add(new BuildingsBufferElementData() {
                        Prefab = GetEntity(tower, TransformUsageFlags.Dynamic)
                    });
                }
                var ghosts = AddBuffer<BuildingGhostsBufferElementData>(entity);
                foreach (var ghost in authoring.ghosts) {
                    ghosts.Add(new BuildingGhostsBufferElementData() {
                        Prefab = GetEntity(ghost, TransformUsageFlags.Dynamic)
                    });
                }
            }
        }
    }
}