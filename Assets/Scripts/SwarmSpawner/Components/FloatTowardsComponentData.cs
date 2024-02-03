using Unity.Entities;
using Unity.Mathematics;
using Random = Unity.Mathematics.Random;

namespace SwarmSpawner.Components {
    public struct FloatTowardsComponentData : IComponentData {
        public float Speed;
        public float ReTargetRate;
        public float3 TargetPoint;
        public float NextReTargetTime;
        public Random Random;
    }
}