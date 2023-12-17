using System.Collections.Generic;
using TowerDefense.Components;
using Unity.Entities;
using UnityEngine;

namespace TowerDefense.MonoBehaviours {
    public class TowerRegisterAuthoring : MonoBehaviour {
        
        [SerializeField] private List<GameObject> towers;
        private class TowerRegisterAuthoringBaker : Baker<TowerRegisterAuthoring> {
            public override void Bake(TowerRegisterAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.None);
                var towers = AddBuffer<TowersBufferElementData>(entity);
                foreach (var tower in authoring.towers) {
                    towers.Add(new TowersBufferElementData() {
                        Prefab = GetEntity(tower, TransformUsageFlags.Dynamic)
                    });
                }
            }
        }
    }
}