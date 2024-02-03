using Unity.Entities;
using UnityEngine;

namespace SimpleCrowdsSpawn.Components {
    public struct SpawnerMarker : IComponentData {
    }
    
    public class SpawnerMarkerAuthoring : MonoBehaviour {
        private class SpawnerMarkerAuthoringBaker : Baker<SpawnerMarkerAuthoring> {
            public override void Bake(SpawnerMarkerAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<SpawnerMarker>(entity);
            }
        }
    }
}