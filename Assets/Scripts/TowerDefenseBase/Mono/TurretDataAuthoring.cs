using TowerDefenseBase.Components;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Authoring;
using UnityEngine;

namespace TowerDefenseBase.Mono {
    public class TurretDataAuthoring : MonoBehaviour {
        
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private float fireRate;
        [SerializeField] private float range;

        private class TowerDataAuthoringBaker : Baker<TurretDataAuthoring> {
            public override void Bake(TurretDataAuthoring authoring) {
                var physicsShapeAuthoring = authoring.bulletPrefab.GetComponent<PhysicsShapeAuthoring>();
                var filter = CollisionFilter.Default;
                filter.CollidesWith = physicsShapeAuthoring.CollidesWith.Value;
                filter.BelongsTo = physicsShapeAuthoring.BelongsTo.Value;

                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new TurretDataComponent {
                    ProjectilePrefab = GetEntity(authoring.bulletPrefab, TransformUsageFlags.Dynamic),
                    ShootTimer = authoring.fireRate,
                });
                
                BlobAssetReference<TurretConfig> configAsset = BlobAssetReference<TurretConfig>.Create(new TurretConfig {
                    Range = authoring.range,
                    ShootFrequency = authoring.fireRate,
                    Filter = filter
                });
                AddBlobAsset(ref configAsset, out var hash);
                
                AddComponent(entity, new TurretConfigAsset {
                    Config = configAsset
                });
            }
        }
    }
}