using Unity.Entities;

namespace CustomParticles.Components {
    
    // This component might be just a tag, its main purpose is to aggregate the total number of particles in the system before destroying it
    public struct ParticleParent : ICleanupSharedComponentData {
        public Entity Parent;
    }
}