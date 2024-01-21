using SimpleCrowdsSpawn.Components;
using SimpleCrowdsSpawn.Systems.Jobs;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using UnityEngine;

namespace SimpleCrowdsSpawn.Systems {
    
    [BurstCompile]
    public partial class CrowdSpawnerAsJobSystem : SystemBase {
        
        private readonly int _maxCrowdSize = 0;
        
        [BurstCompile]
        protected override void OnUpdate() {
            // Check if we have reached the maximum crowd size
            var entityQuery = EntityManager.CreateEntityQuery(typeof(CrowdMemberTag));
            var crowdSize = entityQuery.CalculateEntityCount();
            
            if (crowdSize >= _maxCrowdSize) return;
            Debug.Log($"Crowd size System As job: {crowdSize}");
            
            // Buffer a command to instantiate a crowd member
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(World.Unmanaged);
            
            // Get the prefab - It already has the components present (Archetype)
            var entityFromPrefab = SystemAPI.GetSingleton<CrowdSpawner>().Prefab;

            // var newEntityArchetype = EntityManager.CreateArchetype(
            //     typeof(CrowdMemberTag),
            //     typeof(Value),
            //     typeof(TargetPosition)
            // );
            
            // Get the Random - To alter the speed and target position
            //var randomComponent = SystemAPI.GetSingletonRW<RandomComponent>();
            
            var job = new CreateCrowdMemberJob() {
                PrefabEntity = entityFromPrefab,
                Ecb = ecb.AsParallelWriter(),
                //RandomComponent = randomComponent
            };
            var jobHandle = job.Schedule(100, 64, this.Dependency);
            this.Dependency = jobHandle;
        }
    }

    
    

}