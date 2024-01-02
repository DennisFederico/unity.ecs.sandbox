using Recap101.Components;
using Unity.Entities;
using UnityEngine;

namespace Recap101.Systems {
    
    [DisableAutoCreation]
    [WorldSystemFilter(WorldSystemFilterFlags.BakingSystem)]
    public partial class MyBakingSystem : SystemBase {
        protected override void OnUpdate() {
            Entities
                .WithAll<TagComponent>()
                .ForEach((in DynamicBuffer<WaypointsComponent> waypoints) =>
            {
                Debug.Log($"This entity has {waypoints.Length} waypoints.");
            }).Run();
        }
    }
}