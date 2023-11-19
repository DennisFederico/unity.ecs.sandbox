using Crowds.Components;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Crowds.Systems {
    public partial class CrowdSpawnerSystem : SystemBase {
        protected override void OnUpdate() {
            
            int maxCrowdSize = 20;
            
            // Check if we have reached the maximum crowd size
            var entityQuery = EntityManager.CreateEntityQuery(typeof(CrowdMemberTag));
            var crowdSize = entityQuery.CalculateEntityCount();
            if (crowdSize >= maxCrowdSize) return;
            
            // Buffer a command to instantiate a crowd member
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
            
            // Get the prefab
            var crowdSpawner = SystemAPI.GetSingleton<CrowdSpawner>();
            
            // Get the Random
            var randomComponent = SystemAPI.GetSingletonRW<RandomComponent>();

            //Queue the instance command
            var crowdMember = ecb.Instantiate(crowdSpawner.Prefab);
            ecb.AddComponent(crowdMember, new CrowdMemberTag());
            ecb.SetComponent(crowdMember, new Speed() {
                Value = randomComponent.ValueRW.Value.NextFloat(0.5f, 2f)
            });
            ecb.SetComponent(crowdMember, new TargetPosition {
                Value = new float3 {
                    x = randomComponent.ValueRW.Value.NextFloat(-15f, 15f),
                    y = 0f,
                    z = randomComponent.ValueRW.Value.NextFloat(-15f, 15f)
                }
            });
            // var targetPosition = SystemAPI.GetComponentRW<TargetPosition>(crowdMember);
            // targetPosition.ValueRW.Value = GetRandomTargetPosition(randomComponent);
            // EntityManager.AddComponentData(crowdMember, new TargetPosition {
            //     Value = GetRandomTargetPosition(randomComponent)
            // });
        }
    }
}