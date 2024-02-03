using SimpleCrowdsSpawn.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using Utils.Narkdagas.Ecs;

namespace SimpleCrowdsSpawn.Systems {

    [UpdateBefore(typeof(PlaceSpawnerSystem))]
    public partial struct SpawnSystem : ISystem {

        private EntityQuery _crowdMemberEntitiesQuery;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<CrowdSpawner>();
            state.RequireForUpdate<SpawnRequestBuffer>();
            state.RequireForUpdate<RandomSeeder>();
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            _crowdMemberEntitiesQuery = SystemAPI.QueryBuilder()
                .WithAll<CrowdMemberTag>()
                .Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var spawnRequestBuffer = SystemAPI.GetSingletonBuffer<SpawnRequestBuffer>();
            if (spawnRequestBuffer.Length == 0) return;
            var crowdSpawner = SystemAPI.GetSingleton<CrowdSpawner>();
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);

            foreach (var spawnRequest in spawnRequestBuffer) {
                var requestAmount = spawnRequest.Amount;
                switch (requestAmount) {
                    case > 0:
                        if (crowdSpawner.Spawner == Entity.Null) continue;
                        var spawnerTransform = SystemAPI.GetComponentRO<LocalTransform>(crowdSpawner.Spawner).ValueRO;
                        SpawnEntities(requestAmount, crowdSpawner.Prefab, LocalTransform.FromPosition(spawnerTransform.Position), ecb);
                        break;
                    case < 0:
                        //Note that the requestAmount is negative for despawn, multiply by -1
                        DeSpawnEntities(-requestAmount, SystemAPI.GetSingletonRW<RandomSeeder>(), _crowdMemberEntitiesQuery.ToEntityArray(Allocator.Temp), ecb);
                        break;
                }
            }
            spawnRequestBuffer.Clear();
        }

        private static void SpawnEntities(int requestAmount, Entity prefabEntity, LocalTransform spawnLocation, EntityCommandBuffer ecb) {
            NativeArray<Entity> entities = new NativeArray<Entity>(requestAmount, Allocator.Temp);
            ecb.Instantiate(prefabEntity, entities);
            ecb.AddComponent(entities, spawnLocation);
            ecb.AddComponent(entities, new TargetPosition { Value = spawnLocation.Position });
        }

        private static void DeSpawnEntities(int amount, RefRW<RandomSeeder> random, NativeArray<Entity> entities, EntityCommandBuffer ecb) {
            if (entities.Length <= amount) {
                ecb.DestroyEntity(entities);
            } else {
                var toDelete = new NativeList<Entity>(amount, Allocator.Temp);
                while (amount > 0) {
                    var nextInt = random.ValueRW.NextSeed.NextInt(0, entities.Length);
                    if (toDelete.Contains(entities[nextInt])) continue;
                    toDelete.Add(entities[nextInt]);
                    amount--;
                }
                ecb.DestroyEntity(toDelete.AsArray());
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) {
        }
    }
}