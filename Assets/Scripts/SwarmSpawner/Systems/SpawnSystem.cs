using SwarmSpawner.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace SwarmSpawner.Systems {
    
    
    public partial struct SpawnSystem : ISystem {
        
        private Random _random;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            state.RequireForUpdate<BeginSimulationEntityCommandBufferSystem.Singleton>();
            _random = new Random((uint) (SystemAPI.Time.ElapsedTime + 123456789));
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var ecbSingleton = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>();
            var ecb = ecbSingleton.CreateCommandBuffer(state.WorldUnmanaged);

            foreach (var (transform, spawner, originArea) in 
                     SystemAPI.Query<RefRW<LocalTransform>, RefRW<SpawnComponentData>, RefRW<AreaComponentData>>()) {
                if (SystemAPI.Time.ElapsedTime < spawner.ValueRW.NextSpawnTime) return;
                spawner.ValueRW.NextSpawnTime = (float)(SystemAPI.Time.ElapsedTime + spawner.ValueRW.SpawnRate);
                var entity = ecb.Instantiate(spawner.ValueRW.Prefab);
                var vectorArea = originArea.ValueRW.area / 2f;
                var randomPoint = spawner.ValueRW.InternalRandom.NextFloat3(-vectorArea, vectorArea);
                var randomPos = transform.ValueRW.TransformPoint(randomPoint);
                var randomLocalTransform = LocalTransform.FromPosition(randomPos);
                ecb.AddComponent(entity, randomLocalTransform);
                ecb.AddComponent(entity, new RandomComponent() { Value = new Random(_random.NextUInt()) });
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }
}