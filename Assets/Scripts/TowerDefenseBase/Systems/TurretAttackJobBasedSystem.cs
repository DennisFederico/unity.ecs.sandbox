using System.Collections.Generic;
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
using UnityEngine;

namespace TowerDefenseBase.Systems {
    
    #region Jobs
    [BurstCompile]
    public partial struct ShootProjectileJob : IJobEntity {

        [ReadOnly] public PhysicsWorldSingleton PhysicsWorld;
        public EntityCommandBuffer.ParallelWriter Ecb;
        
        [BurstCompile]
        private void Execute(ref ProjectilePrefabRefComponent projectilePrefabRef, in TurretConfigAsset configAsset, FireRateComponent fireRate, in LocalToWorld towerPos) {
            if (!fireRate.HasElapsed()) return;
            ref var config = ref configAsset.Config.Value;
            var hitList = new NativeList<DistanceHit>(Allocator.Temp);
            AllHitsCollector<DistanceHit> hitCollector = new AllHitsCollector<DistanceHit>(config.Range, ref hitList);
            if (!PhysicsWorld.OverlapSphereCustom(towerPos.Position, config.Range, ref hitCollector, config.Filter)) return;
            //This algorithm can be optimized in many ways, from breaking it into smaller systems to using a spatial partitioning system
            //As an example, we already know they are in range, if the angel is < 180 degrees we right away filter negative dot product values
            //Lets first sort the hits by distance, then filter those out of the FoV angle.
            //We could finally RayCast to check for obstacles (another turret basically) before stopping the loop, but we don't care right now
            //When dealing with a high number of hits we could use a SortJob and the output tof this job would simply be the list to sort later
            hitList.Sort(new DistanceHitCompare());
            var fovRadians = math.radians(config.FovAngle/2);

            foreach (var hit in hitList) {
                var direction = math.normalize(hit.Position - towerPos.Position);
                var radians = math.acos(math.dot(direction, towerPos.Forward));
                if (radians > fovRadians) continue;
                // //RayCast for obstacles
                // var rayCastInput = new RayCastInput() {
                //     Start = towerPos.ValueRO.Position, //TODO offset the Y to the top of the tower
                //     End = hit.Position, //TODO offset the Y to the center of the enemy
                //     Filter = config.Filter //TODO Other than the actual enemy layer
                // };
                // if (physicsWorldSingleton.CastRay(rayCastInput)) continue;

                //Actual shoot
                var bullet = Ecb.Instantiate(0, projectilePrefabRef.ProjectilePrefab);
                Ecb.SetComponent(1, bullet, LocalTransform.FromPosition(towerPos.Position + towerPos.Up));
                //We could get the current position instead of the entity and shoot straight a little ahead of the enemy 
                Ecb.AddComponent(2, bullet, new ProjectileTargetComponent { Value = hit.Entity });
                break; //Remove if we should fire to all targets in range
            }
        }
    }

    [BurstCompile]
    public partial struct MoveProjectileJob : IJobEntity {

        [NativeDisableContainerSafetyRestriction]
        [ReadOnly] public ComponentLookup<LocalTransform> PositionLookup;
        [ReadOnly] public float DeltaTime;
        public EntityCommandBuffer.ParallelWriter Ecb;
        
        //Projectiles could be "tagged" and we could also use an Aspect to query/move it
        [BurstCompile]
        private void Execute(in ProjectileTargetComponent target, in MoveSpeedComponent speed, ref LocalTransform transform, Entity entity) {
            //We should shoot straight to a position instead of "following" the entity
            if (PositionLookup.TryGetComponent(target.Value, out var targetPosition)) {
                float3 direction = targetPosition.Position + math.up() *.5f - transform.Position;
                transform.Position += math.normalize(direction) * (speed.Value * DeltaTime);
                transform.Rotation = quaternion.LookRotation(direction, math.up());
            } else {
                //Should we let it go straight ahead until TTL/Hit something (including the floor)?
                Ecb.DestroyEntity(0, entity);
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
            //Check the type of entities involved in the trigger, there should be one projectile and one enemy
            var projectileEntity = Entity.Null;
            var enemyEntity = Entity.Null;
            
            //This could be optimized in different ways, like adding a tag component to the enemy
            //the gist here is to identify the projectile and a valid enemy
            if (ProjectileImpactLookup.TryGetComponent(triggerEvent.EntityA, out var projectile) && 
                (!ProjectileImpactLookup.HasComponent(triggerEvent.EntityB) && HealthLookup.HasComponent(triggerEvent.EntityB))) {
                projectileEntity = triggerEvent.EntityA;
                enemyEntity = triggerEvent.EntityB;
            } else if (ProjectileImpactLookup.TryGetComponent(triggerEvent.EntityB, out projectile) &&
                       (!ProjectileImpactLookup.HasComponent(triggerEvent.EntityA) && HealthLookup.HasComponent(triggerEvent.EntityA))) {
                projectileEntity = triggerEvent.EntityB;
                enemyEntity = triggerEvent.EntityA;
            } 
            if (Entity.Null.Equals(projectileEntity) || Entity.Null.Equals(enemyEntity)) return;

            //Check if the projectile has hit the target before
            if (HitListLookup.TryGetBuffer(projectileEntity, out var hits)) {
                //Check if already hit
                for (var i = 0; i < hits.Length; i++) {
                    if (hits[i].Entity.Equals(enemyEntity)) return;
                }
                //Add the enemy to the list of already hit entities
                hits.Add(new Hits {Entity = enemyEntity});
            }

            HealthComponent enemyHealth = HealthLookup[enemyEntity];
            HealthComponent bulletDamage = HealthLookup[projectileEntity];
            
            //Deal damage to the enemy, we could assign the damage to a buffer and apply it in another system
            enemyHealth.Value -= bulletDamage.Value;
            HealthLookup[enemyEntity] = enemyHealth;

            //We could move this check to another system and work in chunks of enemies
            if (enemyHealth.Value <= 0) {
                EntityBuffer.DestroyEntity(0, enemyEntity);
            }
            
            //Projectile Hit VFX
            var hitVfx = EntityBuffer.Instantiate(1, projectile.VfxPrefab);
            EntityBuffer.AddComponent(2, hitVfx, LocalTransform.FromPosition(PositionLookup[projectileEntity].Position));
            
            //Destroy the projectile
            EntityBuffer.DestroyEntity(3, projectileEntity);
        }
    }
    #endregion
    
    #region System
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(PhysicsSystemGroup))]
    [BurstCompile]
    public partial struct TurretAttackJobBasedSystem : ISystem {
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
                PhysicsWorld = physicsWorldSingleton,
                Ecb = bosEcb
            }.ScheduleParallel(state.Dependency);
            
            //MOVE BULLET JOB
            state.Dependency = new MoveProjectileJob {
                PositionLookup = _positionLookup,
                DeltaTime = SystemAPI.Time.DeltaTime,
                Ecb = eosEcb
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
    #endregion
    
    #region Utils
    [BurstCompile]
    public struct DistanceHitCompare : IComparer<DistanceHit> {

        [BurstCompile]
        public int Compare(DistanceHit a, DistanceHit b) {
            return math.abs(a.Distance - b.Distance) < 0.0001 ? 0 : a.Distance < b.Distance ? -1 : 1;
        }
    }
    #endregion
}