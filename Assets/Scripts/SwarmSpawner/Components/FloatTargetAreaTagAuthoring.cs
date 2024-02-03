using Unity.Entities;
using UnityEngine;

namespace SwarmSpawner.Components {
    public class FloatTargetAreaTagAuthoring : MonoBehaviour {
        private class FloatTargetAreaTagAuthoringBaker : Baker<FloatTargetAreaTagAuthoring> {
            public override void Bake(FloatTargetAreaTagAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new FloatTargetAreaTag());
            }
        }
    }
}