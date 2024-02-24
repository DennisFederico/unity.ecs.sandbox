using TowerDefense.Components;
using TowerDefenseBase.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

namespace TowerDefense.Systems {
    
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(PhysicsSystemGroup))]
    [BurstCompile]
    public partial struct ProjectileSystem : ISystem {
        private ComponentLookup<LocalTransform> _enemyPositionLookup;
        private ComponentLookup<HealthComponent> _enemyHealthLookup;
        private ComponentLookup<ProjectileImpactComponent> _projectileImpactLookup;
        

        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<PhysicsWorldSingleton>();
            _enemyPositionLookup = SystemAPI.GetComponentLookup<LocalTransform>(true);
            _enemyHealthLookup = SystemAPI.GetComponentLookup<HealthComponent>();
            _projectileImpactLookup = SystemAPI.GetComponentLookup<ProjectileImpactComponent>();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var ecbBos = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);
            var physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            foreach (var (towerData, towerConfigAsset, towerPos) in 
                     SystemAPI.Query<RefRW<TurretDataComponent>, RefRO<TurretConfigAsset>, RefRO<LocalToWorld>>()) {
                towerData.ValueRW.ShootTimer -= SystemAPI.Time.DeltaTime;
                if (towerData.ValueRO.ShootTimer < 0) {
                    ref var config = ref towerConfigAsset.ValueRO.Config.Value;
                    ClosestHitCollector<DistanceHit> closestHitCollector = new ClosestHitCollector<DistanceHit>(config.Range);
                    if (physicsWorldSingleton.OverlapSphereCustom(towerPos.ValueRO.Position, config.Range, ref closestHitCollector, config.Filter)) {
                        towerData.ValueRW.ShootTimer = config.ShootFrequency;
                        Entity bullet = ecbBos.Instantiate(towerData.ValueRO.ProjectilePrefab);

                        ecbBos.SetComponent(bullet, LocalTransform.FromPosition(towerPos.ValueRO.Position + towerPos.ValueRO.Up));
                        ecbBos.AddComponent(bullet, new ProjectileTargetComponent { Value = closestHitCollector.ClosestHit.Entity });
                    }
                }
            }

            _enemyPositionLookup.Update(ref state);
            //Projectiles should be "tagged" and us an Aspect to manipulate them
            foreach (var (target, speed, transform, projectile) in
                     SystemAPI.Query<RefRO<ProjectileTargetComponent>, RefRO<MoveSpeedComponent>, RefRW<LocalTransform>>().WithEntityAccess()) {

                if (_enemyPositionLookup.TryGetComponent(target.ValueRO.Value, out var targetPosition)) {
                    float3 direction = targetPosition.Position - transform.ValueRO.Position;
                    transform.ValueRW.Position += math.normalize(direction) * (speed.ValueRO.Value * SystemAPI.Time.DeltaTime);
                    transform.ValueRW.Rotation = quaternion.LookRotation(direction, math.up());
                } else {
                    ecbBos.DestroyEntity(projectile);
                }
            }
             
            //Need to lookup the enemy position to check if Hit
            _enemyPositionLookup.Update(ref state);
            _enemyHealthLookup.Update(ref state);
            _projectileImpactLookup.Update(ref state);
            //TODO 1. AIM FOR THE CENTER OF THE PHYSICAL SHAPE
            //TODO 2. USE COLLISION INSTEAD OF DISTANCE FOR THE ACTUAL HIT
            foreach (var (target, transform, projectile) in
                     SystemAPI.Query<RefRO<ProjectileTargetComponent>, RefRO<LocalTransform>>().WithEntityAccess()) {
                if (_enemyPositionLookup.TryGetComponent(target.ValueRO.Value, out var targetPosition)) {
                    if (math.distance(targetPosition.Position, transform.ValueRO.Position) < 0.1) {
                        var hp = _enemyHealthLookup[target.ValueRO.Value];
                        hp.Value -= 5;
                        _enemyHealthLookup[target.ValueRO.Value] = hp;
                        if (_projectileImpactLookup.TryGetComponent(projectile, out var projectileData)) {
                            var vfx = ecbBos.Instantiate(projectileData.VfxPrefab);
                            ecbBos.AddComponent(vfx, LocalTransform.FromPosition(transform.ValueRO.Position));
                        }
                        
                        ecbBos.DestroyEntity(projectile);
                        if (!(hp.Value < 0)) continue;
                        ecbBos.DestroyEntity(target.ValueRO.Value);
                    }       
                } else {
                    ecbBos.DestroyEntity(projectile);
                }
            }
        }
    }
}