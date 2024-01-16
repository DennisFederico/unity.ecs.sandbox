using Unity.Entities;

namespace Utils.Narkdagas.Ecs {
    public struct RandomSeeder : IComponentData {
        public Unity.Mathematics.Random NextSeed;
    }
}