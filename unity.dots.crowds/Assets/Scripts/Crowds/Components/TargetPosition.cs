using Unity.Entities;
using Unity.Mathematics;

namespace Crowds.Components {
    public struct TargetPosition : IComponentData {
        public float3 Value;
    }
}