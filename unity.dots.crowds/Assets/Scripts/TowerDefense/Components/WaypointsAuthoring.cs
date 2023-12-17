using System;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace TowerDefense.Components {
    
    [InternalBufferCapacity(4)]
    public struct WaypointsComponent : IBufferElementData {
        public float3 Value;
    }
    
    public class WaypointsAuthoring : MonoBehaviour {
        
        //OR ARRAY OF TRANSFORMS!?
        //[SerializeField] private float3[] waypoints = Array.Empty<float3>();
        [SerializeField] private Transform[] waypoints = Array.Empty<Transform>();
        private class WaypointsAuthoringBaker : Baker<WaypointsAuthoring> {
            public override void Bake(WaypointsAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                var waypoints = AddBuffer<WaypointsComponent>(entity);
                foreach (var t in authoring.waypoints) {
                    waypoints.Add(new WaypointsComponent { Value = t.position });
                }
            }
        }
    }
}