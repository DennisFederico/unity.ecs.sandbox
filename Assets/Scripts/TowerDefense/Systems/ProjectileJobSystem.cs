using TowerDefense.Components;
using TowerDefenseBase.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

namespace TowerDefense.Systems {
    
    [BurstCompile]
    public partial struct ShootProjectileJob : IJobEntity {

        [ReadOnly] public float DeltaTime;
        [ReadOnly] public PhysicsWorldSingleton PhysicsWorld;
        public EntityCommandBuffer.ParallelWriter EntityBuffer;
        
        [BurstCompile]
        private void Execute(ref TurretDataComponent turretData, in TurretConfigAsset configAsset, in LocalToWorld towerPos) {
            turretData.ShootTimer -= DeltaTime;
            if (!(turretData.ShootTimer <= 0)) return;
            ref var config = ref configAsset.Config.Value;
            ClosestHitCollector<DistanceHit> closestHitCollector = new ClosestHitCollector<DistanceHit>(config.Range);
            if (!PhysicsWorld.OverlapSphereCustom(towerPos.Position, config.Range, ref closestHitCollector, config.Filter)) return;
            turretData.ShootTimer = config.ShootFrequency;
            Entity bullet = EntityBuffer.Instantiate(0, turretData.ProjectilePrefab);
            EntityBuffer.SetComponent(1, bullet, LocalTransform.FromPosition(towerPos.Position + towerPos.Up));
            EntityBuffer.AddComponent(2, bullet, new ProjectileTargetComponent { Value = closestHitCollector.ClosestHit.Entity });
        }
    }

    [BurstCompile]
    public partial struct MoveProjectileJob : IJobEntity {

        [NativeDisableContainerSafetyRestriction]
        [ReadOnly] public ComponentLookup<LocalTransform> PositionLookup;
        [ReadOnly] public float DeltaTime;
        public EntityCommandBuffer.ParallelWriter EosEntityBuffer;
        
        [BurstCompile]
        private void Execute(in ProjectileTargetComponent target, in MoveSpeedComponent speed, ref LocalTransform transform, Entity entity) {
            if (PositionLookup.TryGetComponent(target.Value, out var targetPosition)) {
                float3 direction = targetPosition.Position - transform.Position;
                transform.Position += math.normalize(direction) * (speed.Value * DeltaTime);
                transform.Rotation = quaternion.LookRotation(direction, math.up());
            } else {
                //TODO let it go straight ahead until TTL/Hit something (including the floor)?
                EosEntityBuffer.DestroyEntity(0, entity);
            }
        }
    }
    
    [BurstCompile]
    public struct ProjectileHitJob : ITriggerEventsJob {
        
        [ReadOnly] public ComponentLookup<LocalTransform> PositionLookup;
        [ReadOnly] public ComponentLookup<ProjectileImpactComponent> ProjectileImpactLookup;
        public ComponentLookup<HealthComponent> HealthLookup;
        public BufferLookup<Hits> HitListLookup;
        public EntityCommandBuffer.ParallelWriter EntityBuffer;
        
        
        [BurstCompile]
        public void Execute(TriggerEvent triggerEvent) {
            //Check the type of entities involved in the trigger
            //There should be one projectile and one enemy
            // Debug.Log($"TRIGGER EVENT: {triggerEvent.EntityA} - {triggerEvent.EntityB}");
            Entity projectileEntity = Entity.Null;
            Entity enemyEntity = Entity.Null;
            HealthComponent enemyHealth = default;

            if (ProjectileImpactLookup.TryGetComponent(triggerEvent.EntityA, out var projectileVfx) && HealthLookup.TryGetComponent(triggerEvent.EntityB, out enemyHealth)) {
                projectileEntity = triggerEvent.EntityA;
                enemyEntity = triggerEvent.EntityB;
            } else if (ProjectileImpactLookup.TryGetComponent(triggerEvent.EntityB, out projectileVfx) && HealthLookup.TryGetComponent(triggerEvent.EntityA, out enemyHealth)) {
                projectileEntity = triggerEvent.EntityB;
                enemyEntity = triggerEvent.EntityA;                
            } 
            
            if (Entity.Null.Equals(projectileEntity) || Entity.Null.Equals(enemyEntity)) return;

            var totalHits = 1;
            //Check if the projectile can hit more than one enemy
            if (HitListLookup.TryGetBuffer(projectileEntity, out var hits)) {
                //Check if already hit
                for (int i = 0; i < hits.Length; i++) {
                    if (hits[i].Entity.Equals(enemyEntity)) return;
                }
                //Add the enemy to the list of already hit entities
                hits.Add(new Hits {Entity = enemyEntity});
                totalHits = hits.Length;
            }
            
            //Deal damage to the enemy
            enemyHealth.Value -= 5;
            HealthLookup[enemyEntity] = enemyHealth;

            if (enemyHealth.Value <= 0) {
                EntityBuffer.DestroyEntity(0, enemyEntity);
            }
            
            //Projectile Hit VFX
            var hitVfx = EntityBuffer.Instantiate(1, projectileVfx.VfxPrefab);
            EntityBuffer.AddComponent(2, hitVfx, LocalTransform.FromPosition(PositionLookup[projectileEntity].Position));
            
            //Destroy the projectile
            if (projectileVfx.HitsLeft <= totalHits) {
                EntityBuffer.DestroyEntity(3, projectileEntity);
            }
        }
    }
    
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(PhysicsSystemGroup))]
    [BurstCompile]
    public partial struct ProjectileJobSystem : ISystem {
        private ComponentLookup<LocalTransform> _positionLookup;
        private ComponentLookup<HealthComponent> _enemyHealthLookup;
        private ComponentLookup<ProjectileImpactComponent> _projectileImpactLookup;
        private BufferLookup<Hits> _hitListLookup;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<PhysicsWorldSingleton>();
            state.RequireForUpdate<SimulationSingleton>();
            
            _positionLookup = SystemAPI.GetComponentLookup<LocalTransform>(true);
            _enemyHealthLookup = SystemAPI.GetComponentLookup<HealthComponent>();
            _projectileImpactLookup = SystemAPI.GetComponentLookup<ProjectileImpactComponent>(true);
            _hitListLookup = SystemAPI.GetBufferLookup<Hits>();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {   
            var bosEcb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged)
                .AsParallelWriter();
            
            var eosEcb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged)
                .AsParallelWriter();
            
            var physicsWorldSingleton = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            _positionLookup.Update(ref state);
            _enemyHealthLookup.Update(ref state);
            _projectileImpactLookup.Update(ref state);
            _hitListLookup.Update(ref state);
            
            //SHOOT BULLET JOB
            state.Dependency = new ShootProjectileJob {
                DeltaTime = SystemAPI.Time.DeltaTime,
                PhysicsWorld = physicsWorldSingleton,
                EntityBuffer = bosEcb
            }.ScheduleParallel(state.Dependency);
            
            //MOVE BULLET JOB
            state.Dependency = new MoveProjectileJob {
                PositionLookup = _positionLookup,
                DeltaTime = SystemAPI.Time.DeltaTime,
                EosEntityBuffer = eosEcb
            }.ScheduleParallel(state.Dependency);

            //TODO review the execution order for the systems involved in the collision
            //HIT BULLET JOB
            state.Dependency = new ProjectileHitJob {
                PositionLookup = _positionLookup,
                ProjectileImpactLookup = _projectileImpactLookup,
                HealthLookup = _enemyHealthLookup,
                HitListLookup = _hitListLookup,
                EntityBuffer = eosEcb
            }.Schedule(SystemAPI.GetSingleton<SimulationSingleton>(), state.Dependency);
        }
    }
}