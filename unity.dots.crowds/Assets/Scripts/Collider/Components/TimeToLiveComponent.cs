using Unity.Entities;

namespace Collider.Components {
    public struct TimeToLiveComponent : IComponentData {
        public double CreatedAt;
        public float TimeToLive;
    }
}