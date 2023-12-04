using Unity.Entities;

namespace Spawner.Components {
    public struct MovingComponentData : IComponentData {
        public float moveSpeed;
        public float rotateSpeed;
    }
}