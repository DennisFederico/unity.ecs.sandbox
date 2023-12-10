using Towers.Components;
using Unity.Entities;
using UnityEngine;

namespace Towers.Authoring {
    
    [AddComponentMenu("Tower")]
    [DisallowMultipleComponent]
    public class TowerAuthoring : MonoBehaviour {
        public Formation formation;
        public int unitCount;
        public float radius;
        private class TowerAuthoringBaker : Baker<TowerAuthoring> {
            public override void Bake(TowerAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new TowerComponent {
                    Formation = authoring.formation,
                    UnitCount = authoring.unitCount,
                    Radius = authoring.radius
                });
                AddComponent(entity, new SpawnUnitsTag());
            }
        }
    }
}