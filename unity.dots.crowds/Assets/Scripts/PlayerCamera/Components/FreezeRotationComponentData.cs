using Unity.Entities;
using Unity.Mathematics;

namespace PlayerCamera.Components {
    public struct FreezeRotationComponentData : IComponentData {
        public bool3 Flags;
    }
}