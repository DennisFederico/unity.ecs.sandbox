using TowerDefenseBase.Components;
using TowerDefenseHybrid.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Physics.Systems;
using Unity.Transforms;

namespace TowerDefenseHybrid.Systems {
    
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(PhysicsSystemGroup))]
    [BurstCompile]
    public partial struct BuildingPlacementByTransformSystem : ISystem {
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<BuildingsBufferElementData>();
            state.RequireForUpdate<BuildingPlacementByTransform>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            //Entity that has BuildingPlacementByTransform buffer almost always exists, even if it's empty
            var buildingPlacementBuffer = SystemAPI.GetSingletonBuffer<BuildingPlacementByTransform>();
            if (buildingPlacementBuffer.IsEmpty) return;
            
            var towers = SystemAPI.GetSingletonBuffer<BuildingsBufferElementData>();
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            
            foreach (var placementData in buildingPlacementBuffer) {
                if (placementData.BuildingId < 1 || placementData.BuildingId > towers.Length) continue;
                var buildingEntity = ecb.Instantiate(towers[placementData.BuildingId -1].Prefab);
                ecb.SetComponent(buildingEntity, LocalTransform.FromPositionRotation(placementData.Position,placementData.Rotation));
            }
            buildingPlacementBuffer.Clear();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }
}