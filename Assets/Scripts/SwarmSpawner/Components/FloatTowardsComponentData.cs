using Unity.Entities;
using Unity.Mathematics;

namespace SwarmSpawner.Components {
    public struct FloatTowardsComponentData : IComponentData {
        public float Speed;
        public float ReTargetRate;
        public float3 TargetPoint;
        public float NextReTargetTime;
    }
}