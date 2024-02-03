using Unity.Entities;
using Unity.Mathematics;

namespace CustomParticles.Components {
    public struct ParticleSystemConfig : IComponentData {
        public float EmissionRate;
        public float Lifetime;
        public float Size;
        public float4 Color;
        public float Speed;
    }
}