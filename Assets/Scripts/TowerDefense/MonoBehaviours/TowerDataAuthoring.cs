using TowerDefense.Components;
using Unity.Collections;
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
                    ShootTimer = authoring.fireRate,
                });

                // BlobAssetReference<TowerConfig> configAsset;
                // using (var builder = new BlobBuilder(Allocator.Temp)) {
                //     ref var root = ref builder.ConstructRoot<TowerConfig>();
                //     root.ProjectilePrefab = GetEntity(authoring.projectilePrefab, TransformUsageFlags.Dynamic);
                //     root.Range = authoring.range;
                //     root.ShootFrequency = authoring.fireRate;
                //     root.Filter = filter;
                //     configAsset = builder.CreateBlobAssetReference<TowerConfig>(Allocator.Persistent);
                // }
                
                BlobAssetReference<TowerConfig> configAsset = BlobAssetReference<TowerConfig>.Create(new TowerConfig {
                    Range = authoring.range,
                    ShootFrequency = authoring.fireRate,
                    Filter = filter
                });
                AddBlobAsset(ref configAsset, out var hash);
                
                AddComponent(entity, new TowerConfigAsset {
                    Config = configAsset
                });
            }
        }
    }
}