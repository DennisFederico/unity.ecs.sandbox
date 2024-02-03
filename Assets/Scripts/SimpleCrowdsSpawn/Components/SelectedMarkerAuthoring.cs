using Unity.Entities;
using UnityEngine;

namespace SimpleCrowdsSpawn.Components {
    
    public struct SelectedMarker : IComponentData {
    }
    
    public class SelectedMarkerAuthoring : MonoBehaviour {
        private class SelectedMarkerAuthoringBaker : Baker<SelectedMarkerAuthoring> {
            public override void Bake(SelectedMarkerAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<SelectedMarker>(entity);
            }
        }
    }
}