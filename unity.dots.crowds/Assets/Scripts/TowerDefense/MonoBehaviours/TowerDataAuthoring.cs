using TowerDefense.Components;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Authoring;
using UnityEngine;

namespace TowerDefense.MonoBehaviours {
    public class TowerDataAuthoring : MonoBehaviour {
        
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private float fireRate;
        [SerializeField] private float range;

        private class TowerDataAuthoringBaker : Baker<TowerDataAuthoring> {
            public override void Bake(TowerDataAuthoring authoring) {
                var physicsShapeAuthoring = authoring.projectilePrefab.GetComponent<PhysicsShapeAuthoring>();
                var filter = CollisionFilter.Default;
                filter.CollidesWith = physicsShapeAuthoring.CollidesWith.Value;
                filter.BelongsTo = physicsShapeAuthoring.BelongsTo.Value;

                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new TowerDataComponent {
                    ProjectilePrefab = GetEntity(authoring.projectilePrefab, TransformUsageFlags.Dynamic),
                    Range = authoring.range,
                    ShootTimer = authoring.fireRate,
                    ShootFrequency = authoring.fireRate,
                    Filter = filter
                });
            }
        }
    }
}