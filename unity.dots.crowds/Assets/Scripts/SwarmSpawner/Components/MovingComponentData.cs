using Unity.Entities;

namespace SwarmSpawner.Components {
    public struct MovingComponentData : IComponentData {
        public float MoveSpeed;
        public float RotateSpeed;
    }
}