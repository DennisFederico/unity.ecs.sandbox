using Crowds.Components;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

namespace Crowds.Systems {
    [DisableAutoCreation]
    [BurstCompile]
    public partial class CrowdSpawnerSystem : SystemBase {
        
        [BurstCompile]
        protected override void OnUpdate() {
            
            int maxCrowdSize = 10000;
            
            // Check if we have reached the maximum crowd size
            var entityQuery = EntityManager.CreateEntityQuery(typeof(CrowdMemberTag));
            var crowdSize = entityQuery.CalculateEntityCount();
            if (crowdSize >= maxCrowdSize) return;
            
            Debug.Log($"Crowd size System: {crowdSize}");
            
            // Buffer a command to instantiate a crowd member
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
            
            // Get the prefab - It already has the components present (Archetype)
            var crowdSpawner = SystemAPI.GetSingleton<CrowdSpawner>();
            
            // Get the RandomSeed to add a random seed to the component of the entity, used later to alter the speed and target position
            var randomSeeder = SystemAPI.GetSingletonRW<RandomSeeder>();
            
            //Queue the instantiation command
            for (int i = 0; i < 500; i++) {
                var crowdMember = ecb.Instantiate(crowdSpawner.Prefab);
                // ecb.SetComponent(crowdMember, new RandomComponent() {
                //     Value = Unity.Mathematics.Random.CreateFromIndex(randomSeeder.ValueRW.NextSeed.NextUInt())
                // });
                // ecb.SetComponent(crowdMember, new Speed() {
                //     Value = randomComponent.ValueRW.Value.NextFloat(1f, 3f)
                // });
                // ecb.SetComponent(crowdMember, new TargetPosition {
                //     Value = Utils.Utils.NewRandomPosition(randomComponent.ValueRW.Value)
                // });
            }
        }
    }
}