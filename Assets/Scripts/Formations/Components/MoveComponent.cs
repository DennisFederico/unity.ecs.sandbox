using Unity.Entities;
using Unity.Mathematics;

namespace Formations.Components {
    public struct MoveComponent : IComponentData {
        public float3 TargetPosition;
        public float Speed;
    }
}