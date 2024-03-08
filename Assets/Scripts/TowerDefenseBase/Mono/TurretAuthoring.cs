using TowerDefenseBase.Components;
using TowerDefenseBase.Scriptables;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Authoring;
using UnityEngine;

namespace TowerDefenseBase.Mono {
    public class TurretAuthoring : MonoBehaviour {
        
        [SerializeField] private TurretDataSO turretDataSO;

        private class TowerDataAuthoringBaker : Baker<TurretAuthoring> {
            public override void Bake(TurretAuthoring authoring) {
                DependsOn(authoring.turretDataSO);
                if (authoring.turretDataSO == null) {
                    Debug.Log("Cannot Bake turret. TurretDataSO is null.");
                    return;
                }
                
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                
                var timeBetweenShots = 1f / authoring.turretDataSO.shootsPerSecond;
                var physicsShapeAuthoring = authoring.turretDataSO.bulletPrefab.GetComponent<PhysicsShapeAuthoring>();
                var filter = CollisionFilter.Default;
                filter.CollidesWith = physicsShapeAuthoring.CollidesWith.Value;
                filter.BelongsTo = physicsShapeAuthoring.BelongsTo.Value;
                
                BlobAssetReference<TurretConfig> configAsset = BlobAssetReference<TurretConfig>.Create(new TurretConfig {
                    Range = authoring.turretDataSO.fovRange,
                    Filter = filter,
                    FovAngle = authoring.turretDataSO.fovAngle
                });
                
                //SHOULD WE MAKE THIS BLOB ASSET A SHARED COMPONENT?
                AddBlobAsset(ref configAsset, out _);
                AddComponent(entity, new TurretConfigAsset {
                    Config = configAsset
                });
                AddComponent(entity, new ProjectilePrefabRefComponent {
                    ProjectilePrefab = GetEntity(authoring.turretDataSO.bulletPrefab, TransformUsageFlags.Dynamic),
                });
                AddComponent(entity, new FireRateComponent {
                    MaxTimer = timeBetweenShots,
                    CurrentTimer = timeBetweenShots
                });
            }
        }
    }
}