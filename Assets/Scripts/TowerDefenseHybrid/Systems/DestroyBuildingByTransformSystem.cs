using TowerDefenseHybrid.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Physics.Systems;

namespace TowerDefenseHybrid.Systems {
    
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    [UpdateAfter(typeof(PhysicsSystemGroup))]
    [BurstCompile]
    public partial struct DestroyBuildingByTransformSystem : ISystem {
        
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            state.RequireForUpdate<PhysicsWorldSingleton>();
            state.RequireForUpdate<BuildingDestroyByTransform>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var buildingDestroyBuffer = SystemAPI.GetSingletonBuffer<BuildingDestroyByTransform>();
            if (buildingDestroyBuffer.IsEmpty) return;
            
            //We delete the building entity at the end of the frame
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            var pws = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            
            foreach (var destroyData in buildingDestroyBuffer) {
                //We only have the position where the building should be destroyed
                //We will use physics to find the building entity using a sphere cast
                //TODO to be consistent with the GRID based approach, we should keep a grid of the entities
                //and match with the Grid coordinate
                var hits = new NativeList<DistanceHit>(Allocator.Temp);
                
                var collisionFilter = new CollisionFilter() {
                    BelongsTo = 1u, //This is the layer of the User Input (usually 1)
                    CollidesWith = destroyData.ShapeTag.Value, //This is the layer of the building
                    GroupIndex = 0
                };

                if (!pws.OverlapSphere(destroyData.Position, 1f, ref hits, collisionFilter)) continue;
                foreach (var hit in hits) {
                    ecb.DestroyEntity(hit.Entity);
                }
                
            }
            buildingDestroyBuffer.Clear();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {

        }
    }
}