using Unity.Entities;
using Unity.Transforms;

namespace PlayerCamera.Systems {
    public partial struct SyncFollowerSystem : ISystem {
        
        //Cannot use BurstCompile here because we are using UnityEngine.Object.Instantiate
        public void OnUpdate(ref SystemState state) {
            //Note that we are using a reference already (class instead of struct)
            foreach (var (spawn, transform) in SystemAPI.Query<
                         SpawnFollowerComponentData,
                         RefRO<LocalTransform>>()) {
                //Is prefab set?
                if (spawn.Instance != null) {
                    if (!spawn.Spawned) {
                        //Instantiate prefab
                        spawn.Instance = UnityEngine.Object.Instantiate(spawn.Instance);
                        spawn.Spawned = true;
                    }
                    
                    if (spawn.Position)
                        spawn.Instance.transform.position = transform.ValueRO.Position;
                    if (spawn.Rotation)
                        spawn.Instance.transform.rotation = transform.ValueRO.Rotation;
                }
            }
        }
    }
}