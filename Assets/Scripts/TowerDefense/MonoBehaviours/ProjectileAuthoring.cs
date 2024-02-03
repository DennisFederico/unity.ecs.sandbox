using TowerDefense.Components;
using Unity.Entities;
using UnityEngine;

namespace TowerDefense.MonoBehaviours {
    public class ProjectileAuthoring : MonoBehaviour {
        
        [SerializeField] private float speed;
        [SerializeField] private GameObject impactVfxPrefab;
        [SerializeField] private int numHits = 1;
        [SerializeField] private float timeToLive = 5f;
        
        private class ProjectileAuthoringBaker : Baker<ProjectileAuthoring> {
            public override void Bake(ProjectileAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new MoveSpeedComponent() {
                    Value = authoring.speed
                });
                AddComponent(entity, new ProjectileImpactComponent {
                    VfxPrefab = GetEntity(authoring.impactVfxPrefab, TransformUsageFlags.Dynamic),
                    HitsLeft = authoring.numHits
                });
                AddComponent(entity, new TimeToLiveComponent {
                    Value = authoring.timeToLive
                });
                if (authoring.numHits > 1) {
                    AddBuffer<Hits>(entity);
                }
            }
        }
    }
}