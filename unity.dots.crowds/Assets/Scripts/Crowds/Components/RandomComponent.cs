using Unity.Entities;

namespace Crowds.Components {
    public struct RandomComponent : IComponentData {
        public Unity.Mathematics.Random Value;
    }
}