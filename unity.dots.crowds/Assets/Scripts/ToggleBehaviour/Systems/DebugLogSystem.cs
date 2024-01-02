using System;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace ToggleBehaviour.Systems {
    
    [DisableAutoCreation]
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public partial class DebugLogSystem : SystemBase {
        
        public struct DebugLogDataComponent : IComponentData {
            public float ElapsedTime;
            public FixedString64Bytes Message;
        }
        
        public event EventHandler DebugLogEvent;
        
        protected override void OnUpdate() {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(World.Unmanaged);
            
            Entities
                .WithoutBurst()
                .WithAll<DebugLogDataComponent>()
                .ForEach((Entity entity, in DebugLogDataComponent debugLogData) => {
                    Debug.Log($"{debugLogData.ElapsedTime} -> {debugLogData.Message}");
                    DebugLogEvent?.Invoke(this, EventArgs.Empty);   
                    ecb.DestroyEntity(entity);
                }).Run();
        }
        
        
    }
}