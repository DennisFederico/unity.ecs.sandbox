using Unity.Entities;

namespace CustomParticles.Components {
    public struct ParticleSystemData : IComponentData {
        public double LastEmitTime;
        public int LiveCount;
        public int EmittedCount;
    }
}