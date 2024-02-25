using TowerDefenseEcs.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;

namespace TowerDefenseEcs.Systems {

    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(PhysicsSystemGroup))]
    [BurstCompile]
    public partial struct DestroyBuildingByRayInputSystem : ISystem {

        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<PhysicsWorldSingleton>();
            state.RequireForUpdate<DestroyBuildingData>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var buildingDestroyBuffer = SystemAPI.GetSingletonBuffer<DestroyBuildingData>();
            if (buildingDestroyBuffer.IsEmpty) return;

            //We delete the building entity at the end of the frame
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            var pws = SystemAPI.GetSingleton<PhysicsWorldSingleton>();

            foreach (var destroyData in buildingDestroyBuffer) {
                if (!pws.PhysicsWorld.CastRay(destroyData.Value, out var hit)) continue;
                ecb.DestroyEntity(hit.Entity);
            }

            buildingDestroyBuffer.Clear();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {
        }
    }
}