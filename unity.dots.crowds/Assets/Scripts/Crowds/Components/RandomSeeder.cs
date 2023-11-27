using Unity.Entities;

namespace Crowds.Components {
    public struct RandomSeeder : IComponentData {
        public Unity.Mathematics.Random NextSeed;
    }
}