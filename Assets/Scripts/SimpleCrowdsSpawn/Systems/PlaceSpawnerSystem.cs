using System;
using SimpleCrowdsSpawn.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Utils.Narkdagas.Ecs;

namespace SimpleCrowdsSpawn.Systems {

    [BurstCompile]
    public partial class PlaceSpawnerSystem : SystemBase {

        public event Action<Entity> EntitySelected;
        private EntityQuery _crowdMemberEntitiesQuery;

        protected override void OnCreate() {
            RequireForUpdate<PlaceSpawnerRequestBuffer>();
            RequireForUpdate<CrowdSpawnerPrefabs>();
            RequireForUpdate<CrowdSpawner>();
            RequireForUpdate<RandomSeeder>();
            RequireForUpdate<EndSimulationEntityCommandBufferSystem.Singleton>();
            _crowdMemberEntitiesQuery = SystemAPI.QueryBuilder()
                .WithAll<CrowdMemberTag>()
                .Build();
        }

        protected override void OnUpdate() {
            var placementRequestBuffer = SystemAPI.GetSingletonBuffer<PlaceSpawnerRequestBuffer>();
            if (placementRequestBuffer.Length == 0) return;

            var crowdSpawnerPrefabs = SystemAPI.GetSingleton<CrowdSpawnerPrefabs>();
            var spawnerReference = SystemAPI.GetSingletonRW<CrowdSpawner>();
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
            var randomSeeder = SystemAPI.GetSingletonRW<RandomSeeder>();

            foreach (var placementRequest in placementRequestBuffer) {
                Entity previousSelection = spawnerReference.ValueRW.Spawner;
                Entity selectedEntity;
                if (placementRequest.SelectRandom) {
                    var crowdMembers = _crowdMemberEntitiesQuery.ToEntityArray(Allocator.Temp);
                    selectedEntity = _crowdMemberEntitiesQuery.IsEmpty ? Entity.Null : GetRandomCrowdMemberEntity(crowdMembers, randomSeeder, previousSelection);
                } else {
                    selectedEntity = GetOrCreateMarkerEntity(crowdSpawnerPrefabs.SpawnerMarker, ecb, placementRequest.Position, placementRequest.Rotation);
                }

                if (selectedEntity != Entity.Null) {
                    ProcessEntitySelection(ecb, previousSelection, selectedEntity);
                    spawnerReference.ValueRW.Spawner = selectedEntity;
                    EntitySelected?.Invoke(selectedEntity);
                }
            }
            placementRequestBuffer.Clear();
        }

        private void ProcessEntitySelection(EntityCommandBuffer ecb, Entity previousSelection, Entity selectedEntity) {
            if (previousSelection != Entity.Null) {
                ecb.RemoveComponent<SelectedMarker>(previousSelection);
            }

            ecb.AddComponent<SelectedMarker>(selectedEntity);
        }

        private Entity GetOrCreateMarkerEntity(Entity markerPrefab, EntityCommandBuffer ecb, float3 position, quaternion rotation) {
            if (!SystemAPI.TryGetSingletonEntity<SpawnerMarker>(out var selectedEntity)) {
                //Must use the entity manager since the ecb returns an entity placeholder and cannot be used in the Action invoked by EntitySelected
                selectedEntity = EntityManager.Instantiate(markerPrefab);
            }

            ecb.SetComponent(selectedEntity, LocalTransform.FromPositionRotation(position, rotation));
            return selectedEntity;
        }

        private Entity GetRandomCrowdMemberEntity(NativeArray<Entity> crowdMembers, RefRW<RandomSeeder> randomSeeder, Entity previousSelection, int maxAttempts = 5) {
            var selectedEntity = crowdMembers.Length == 0 ? Entity.Null : crowdMembers[randomSeeder.ValueRW.NextSeed.NextInt(0, crowdMembers.Length)];
            return selectedEntity != previousSelection ? 
                selectedEntity : 
                maxAttempts > 0 ? GetRandomCrowdMemberEntity(crowdMembers, randomSeeder, previousSelection, maxAttempts - 1) : selectedEntity;
        }
    }
}