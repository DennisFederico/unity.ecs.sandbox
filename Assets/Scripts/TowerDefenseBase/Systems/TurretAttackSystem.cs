using System.Collections.Generic;
using TowerDefense.Components;
using TowerDefenseBase.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;

namespace TowerDefenseBase.Systems {

    //
    // NOTE: THIS SYSTEM IS "DISABLED" IN FAVOR OF TurretAttackJobBasedSystem
    // THE OTHER SYSTEM IS NOT NECESSARILY MORE PERFORMANT GIVEN SHORT AMOUNT OF ENTITIES
    //
    [DisableAutoCreation]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(PhysicsSystemGroup))]
    [BurstCompile]
    public partial struct ProjectileSystem : ISystem {
        private ComponentLookup<LocalTransform> _positionLookup;
        private ComponentLookup<HealthComponent> _healthLookup;
        private ComponentLookup<ProjectileImpactComponent> _projectileImpactLookup;
        private BufferLookup<Hits> _hitListLookup;
        

        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<PhysicsWorldSingleton>();
            state.RequireForUpdate<SimulationSingleton>();
            state.RequireForUpdate<EnemyTag>();
            _positionLookup = SystemAPI.GetComponentLookup<LocalTransform>(true);
            _healthLookup = SystemAPI.GetComponentLookup<HealthComponent>();
            _hitListLookup = SystemAPI.GetBufferLookup<Hits>();
            _projectileImpactLookup = SystemAPI.GetComponentLookup<ProjectileImpactComponent>();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            
            //1. SphereOverlap to find the closest enemies (3-5)?
            //For each enemy in range
            //2. Check Angle (dot product?) to see if the enemy in front and in the attack area
            //3. RayCast for obstacles
            //4. If all conditions are met - Shoot
            //TODO - ADD A LOCKING SYSTEM TO KEEP FIRING AT THE SAME TARGET UNTIL OUT OF RANGE OR SIGHT?
            
            var ecbBos = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            var physicsWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            
            //FIRING
            foreach (var (fireTimer, towerData, towerConfigAsset, towerPos) in 
                     SystemAPI.Query<RefRO<FireRateComponent>, RefRW<ProjectilePrefabRefComponent>, RefRO<TurretConfigAsset>, RefRO<LocalToWorld>>()) {
                
                if (!fireTimer.ValueRO.HasElapsed()) continue;
                ref var config = ref towerConfigAsset.ValueRO.Config.Value;
                // ClosestHitCollector<DistanceHit> hitCollector = new ClosestHitCollector<DistanceHit>(config.Range);
                var hitList = new NativeList<DistanceHit>(1, Allocator.Temp);
                AllHitsCollector<DistanceHit> hitCollector = new AllHitsCollector<DistanceHit>(config.Range, ref hitList);
                if (physicsWorld.OverlapSphereCustom(towerPos.ValueRO.Position, config.Range, ref hitCollector, config.Filter)) {
                    //This algorithm can be optimized in many ways, from breaking it into smaller systems to using a spatial partitioning system
                    //As an example, we already know they are in range, if the angel is < 180 degrees we right away filter negative dot product values
                        
                    //Lets first sort the hits by distance, then filter those out of the FoV angle.
                    //We could finally RayCast to check for obstacles (another turret basically) before stopping the loop, but we don't care right now
                    hitList.Sort(new DistanceHitComparer());
                    var fovRadians = math.radians(config.FovAngle/2);
                    foreach (var hit in hitList) {
                        var direction = math.normalize(hit.Position - towerPos.ValueRO.Position);
                        var radians = math.acos(math.dot(direction, towerPos.ValueRO.Forward));
                        if (radians > fovRadians) continue;
                        // //RayCast for obstacles
                        // var raycastInput = new RaycastInput() {
                        //     Start = towerPos.ValueRO.Position, //TODO offset the Y to the top of the tower
                        //     End = hit.Position, //TODO offset the Y to the center of the enemy
                        //     Filter = config.Filter //TODO Other than the actual enemy layer
                        // };
                        // if (physicsWorldSingleton.CastRay(raycastInput)) continue;
                            
                        //Actual shoot
                        var bullet = ecbBos.Instantiate(towerData.ValueRO.ProjectilePrefab);
                        ecbBos.SetComponent(bullet, LocalTransform.FromPosition(towerPos.ValueRO.Position + towerPos.ValueRO.Up));
                        //We could get the current position instead of the entity and shoot straight a little ahead of the enemy 
                        ecbBos.AddComponent(bullet, new ProjectileTargetComponent { Value = hit.Entity });
                        break; //Remove if we should fire to all targets in range
                    }
                }
            }
            
            //Move the projectiles
            _positionLookup.Update(ref state);
            //Projectiles could be "tagged" and we could also use an Aspect to query/move it
            foreach (var (target, speed, transform, projectile) in
                     SystemAPI.Query<RefRO<ProjectileTargetComponent>, RefRO<MoveSpeedComponent>, RefRW<LocalTransform>>().WithEntityAccess()) {
                //We should shoot straight to a position instead of "following" the entity
                if (_positionLookup.TryGetComponent(target.ValueRO.Value, out var targetPosition)) {
                    float3 direction = (targetPosition.Position + math.up() *.5f) - transform.ValueRO.Position;
                    transform.ValueRW.Position += math.normalize(direction) * (speed.ValueRO.Value * SystemAPI.Time.DeltaTime);
                    transform.ValueRW.Rotation = quaternion.LookRotation(direction, math.up());
                } else {
                    ecbBos.DestroyEntity(projectile); //Should we let it go straight ahead until TTL/Hit something (including the floor)?
                }
            }
             
            //Process HIT
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            _healthLookup.Update(ref state);
            _projectileImpactLookup.Update(ref state);
            _hitListLookup.Update(ref state);
            var projectileEntity = Entity.Null;
            var enemyEntity = Entity.Null;
            var simulation = SystemAPI.GetSingleton<SimulationSingleton>().AsSimulation();
            var triggerEvents = simulation.TriggerEvents;
            foreach (var triggerEvent in triggerEvents) {
                if (_projectileImpactLookup.TryGetComponent(triggerEvent.EntityA, out var projectile) &&
                    (!_projectileImpactLookup.HasComponent(triggerEvent.EntityB) && _healthLookup.HasComponent(triggerEvent.EntityB))) {
                    projectileEntity = triggerEvent.EntityA;
                    enemyEntity = triggerEvent.EntityB;
                } else if (_projectileImpactLookup.TryGetComponent(triggerEvent.EntityB, out projectile) &&
                           (!_projectileImpactLookup.HasComponent(triggerEvent.EntityA) && _healthLookup.HasComponent(triggerEvent.EntityA))) {
                    projectileEntity = triggerEvent.EntityB;
                    enemyEntity = triggerEvent.EntityA;
                }

                if (Entity.Null.Equals(projectileEntity) || Entity.Null.Equals(enemyEntity)) {
                    Debug.Log("No projectile or enemy found");
                    continue;
                }

                bool processHit = true;
                //Check if the projectile has hit the target before
                if (_hitListLookup.TryGetBuffer(projectileEntity, out var hits)) {
                    //Check if already hit
                    for (var i = 0; i < hits.Length; i++) {
                        if (!hits[i].Entity.Equals(enemyEntity)) continue;
                        processHit = false;
                        break;
                    }
                    //Add the enemy to the list of already hit entities
                    hits.Add(new Hits {Entity = enemyEntity});
                }

                if (!processHit) continue;
                //Process Damage
                HealthComponent enemyHealth = _healthLookup[enemyEntity];
                HealthComponent bulletDamage = _healthLookup[projectileEntity];
                enemyHealth.Value -= bulletDamage.Value;
                _healthLookup[enemyEntity] = enemyHealth;
                
                if (enemyHealth.Value <= 0) {
                    ecb.DestroyEntity(enemyEntity);
                }
            
                //Projectile Hit VFX
                var hitVfx = ecbBos.Instantiate(projectile.VfxPrefab);
                ecbBos.AddComponent(hitVfx, LocalTransform.FromPosition(_positionLookup[projectileEntity].Position));
            
                //Destroy the projectile
                ecb.DestroyEntity(projectileEntity);
            }
        }
    }
    
    [BurstCompile]
    public struct DistanceHitComparer : IComparer<DistanceHit> {

        [BurstCompile]
        public int Compare(DistanceHit a, DistanceHit b) {
            return math.abs(a.Distance - b.Distance) < 0.0001 ? 0 : a.Distance < b.Distance ? -1 : 1;
        }
    }
}