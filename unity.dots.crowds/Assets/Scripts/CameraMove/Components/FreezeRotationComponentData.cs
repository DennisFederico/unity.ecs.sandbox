using Unity.Entities;
using Unity.Mathematics;

namespace CameraMove.Components {
    public struct FreezeRotationComponentData : IComponentData {
        public bool3 Flags;
    }
}