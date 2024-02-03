using Unity.Entities;

namespace PlayerCamera.Components {
    public struct MovableComponentData : IComponentData {
        public float MoveSpeed;
        public float RotateSpeed;
    }
}