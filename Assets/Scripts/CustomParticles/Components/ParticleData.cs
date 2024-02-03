using Unity.Entities;

namespace CustomParticles.Components {
    public struct ParticleData : IComponentData {
        public float Age;
        public float Lifetime;
        public float AgeOverLifetime; // 0..1 used to animate some modifiers that change over time
    }
}