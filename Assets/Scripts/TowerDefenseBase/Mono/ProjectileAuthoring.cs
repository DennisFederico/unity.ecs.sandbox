using TowerDefenseBase.Components;
using Unity.Entities;
using UnityEngine;

namespace TowerDefenseBase.Mono {
    public class ProjectileAuthoring : MonoBehaviour {
        
        [SerializeField] private float speed;
        [SerializeField] private GameObject impactVfxPrefab;
        [SerializeField] private float damage = 5f;
        [SerializeField] private float timeToLive = 5f;
        
        private class ProjectileAuthoringBaker : Baker<ProjectileAuthoring> {
            public override void Bake(ProjectileAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new MoveSpeedComponent() {
                    Value = authoring.speed
                });
                //We re-use the HealthComponent for the damage of the projectile
                AddComponent(entity, new HealthComponent() {
                    Value = authoring.damage
                });
                AddComponent(entity, new ProjectileImpactComponent {
                    VfxPrefab = GetEntity(authoring.impactVfxPrefab, TransformUsageFlags.Dynamic),
                });
                //I really think there's no point on adding this buffer the collision for a given frame 
                //would only happen once for the projectile against other colliders, and we remove the projectile
                //after the first collision, so we shouldn't need to store multiple hits
                AddBuffer<Hits>(entity);
                AddComponent(entity, new TimeToLiveComponent {
                    Value = authoring.timeToLive
                });
            }
        }
    }
}