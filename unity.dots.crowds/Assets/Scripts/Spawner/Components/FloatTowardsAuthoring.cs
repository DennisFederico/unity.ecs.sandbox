using Unity.Entities;
using UnityEngine;

namespace Spawner.Components {
    public class FloatTowardsAuthoring : MonoBehaviour {
        
        public float speed;
        public float reTargetRate;
        
        private class FloatTowardsAuthoringBaker : Baker<FloatTowardsAuthoring> {
            public override void Bake(FloatTowardsAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new FloatTowardsComponentData {
                    Speed = authoring.speed,
                    ReTargetRate = authoring.reTargetRate,
                });
            }
        }
    }
}