using Unity.Entities;

namespace AStar.Components {
    public struct PathFollowerTimeToLive : IComponentData {
        public double StartTime;
        public float TimeToLive;
    }
}