using Unity.Entities;

namespace SimpleCrowdsSpawn.Components {
    public struct RandomComponent : IComponentData {
        public Unity.Mathematics.Random Value;
    }
}