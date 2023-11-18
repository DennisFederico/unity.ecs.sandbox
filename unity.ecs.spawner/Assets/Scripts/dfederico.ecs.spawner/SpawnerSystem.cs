using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Transforms;

namespace dfederico.ecs.spawner {
    public partial struct SpawnerSystem : ISystem {
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
        
        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            // This query runs in the main thread
            // //Queries for all "Spawner" Components.
            // foreach (var spawner in SystemAPI.Query<RefRW<Spawner>>()) {
            //     ProcessSpawnerMainThread(ref state, spawner);
            // }
            
            // The following approach queues creation commands from multiple threads
            var ecb = GetEntityCommandBuffer(ref state);
            // Create a new instance of a job and schedules it in parallel
            new ProcessSpawnerJob {
                ElapsedTime = SystemAPI.Time.ElapsedTime,
                Ecb = ecb
            }.ScheduleParallel();
        }

        private EntityCommandBuffer.ParallelWriter GetEntityCommandBuffer(ref SystemState state) {
            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            return ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter();
        }

        private void ProcessSpawnerMainThread(ref SystemState state, RefRW<Spawner> spawner) {
            //Check if spawn time has elapsed
            if (spawner.ValueRO.NextSpawnTime < SystemAPI.Time.ElapsedTime) {
                //Spawn a new entity an position at spawner
                var newSpawn = state.EntityManager.Instantiate(spawner.ValueRO.Prefab);
                state.EntityManager.SetComponentData(newSpawn, LocalTransform.FromPosition(spawner.ValueRO.SpawnerCenter));
                spawner.ValueRW.NextSpawnTime = (float) SystemAPI.Time.ElapsedTime + spawner.ValueRO.SpawnRate;
            }
        }
    }

    [BurstCompile]
    public partial struct ProcessSpawnerJob : IJobEntity {

        public EntityCommandBuffer.ParallelWriter Ecb;
        public double ElapsedTime;
        
        public void Execute([ChunkIndexInQuery] int chunkIndex, ref Spawner spawner) {
            if (spawner.NextSpawnTime < ElapsedTime) {
                var newSpawn = Ecb.Instantiate(chunkIndex, spawner.Prefab);
                Ecb.SetComponent(chunkIndex, newSpawn, LocalTransform.FromPosition(spawner.SpawnerCenter));
                spawner.NextSpawnTime = (float)ElapsedTime + spawner.SpawnRate;
            }
        }
    }
}