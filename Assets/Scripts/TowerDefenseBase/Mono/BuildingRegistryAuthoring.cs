using System.Collections.Generic;
using TowerDefenseBase.Components;
using Unity.Entities;
using UnityEngine;

namespace TowerDefenseBase.Mono {
    public class BuildingRegistryAuthoring : MonoBehaviour {
        
        [SerializeField] private List<GameObject> towers;
        private class TowerRegisterAuthoringBaker : Baker<BuildingRegistryAuthoring> {
            public override void Bake(BuildingRegistryAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.None);
                var towers = AddBuffer<BuildingsBufferElementData>(entity);
                foreach (var tower in authoring.towers) {
                    towers.Add(new BuildingsBufferElementData() {
                        Prefab = GetEntity(tower, TransformUsageFlags.Dynamic)
                    });
                }
            }
        }
    }
}