using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace sandbox {
    public struct TargetPosition : IComponentData {
        public float3 Value;
    }

    public class TargetPositionAuthoring : MonoBehaviour {
        public float3 value;

        public class TargetPositionBaker : Baker<TargetPositionAuthoring> {
            public override void Bake(TargetPositionAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new TargetPosition { Value = authoring.value });
            }
        }
    }
}