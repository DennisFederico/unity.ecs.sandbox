using Unity.Entities;
using Unity.Mathematics;

namespace SwarmSpawner.Components {
    public struct RandomComponent : IComponentData {
        public Random Value;
    }
}