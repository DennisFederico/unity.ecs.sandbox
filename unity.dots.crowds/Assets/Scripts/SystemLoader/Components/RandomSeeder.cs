using Unity.Entities;
using Unity.Mathematics;

namespace SystemLoader.Components {
    public struct RandomSeeder : IComponentData {
        public Random Value;
    }
}