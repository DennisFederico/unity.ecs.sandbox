using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Switching.Systems {
    
    // [DisableAutoCreation]
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    [RequireMatchingQueriesForUpdate]
    public partial class DebugLogSystem : SystemBase {
        
        public struct DebugLogDataComponent : IComponentData {
            public float ElapsedTime;
            public FixedString64Bytes Message;
        }
        
        public event Action<FixedString128Bytes> DebugLogEvent;
        
        [BurstDiscard]
        protected override void OnUpdate() {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
            
            Entities
                .WithoutBurst()
                .WithAll<DebugLogDataComponent>()
                .ForEach((Entity entity, in DebugLogDataComponent debugLogData) => {
                    FixedString128Bytes message = $"{debugLogData.ElapsedTime} -> {debugLogData.Message}";
                    // ReSharper disable once Unity.BurstLoadingManagedType
                    DebugLogEvent?.Invoke(message);   
                    ecb.DestroyEntity(entity);
                }).Run();
        }
    }
}