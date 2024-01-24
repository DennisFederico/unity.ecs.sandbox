using System;
using Unity.Collections;
using Unity.Entities;

namespace Switching.Systems {
    
    // [DisableAutoCreation]
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public partial class DebugLogSystem : SystemBase {
        
        public struct DebugLogDataComponent : IComponentData {
            public float ElapsedTime;
            public FixedString64Bytes Message;
        }
        
        public event EventHandler<FixedString128Bytes> DebugLogEvent;
        
        protected override void OnUpdate() {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
            
            Entities
                .WithoutBurst()
                .WithAll<DebugLogDataComponent>()
                .ForEach((Entity entity, in DebugLogDataComponent debugLogData) => {
                    FixedString128Bytes message = $"{debugLogData.ElapsedTime} -> {debugLogData.Message}";
                    DebugLogEvent?.Invoke(this, message);   
                    ecb.DestroyEntity(entity);
                }).Run();
        }
    }
}