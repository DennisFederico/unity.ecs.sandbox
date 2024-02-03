using Unity.Entities;
using Unity.Mathematics;

namespace AStar.Components {
    public struct PathFindingRequest : IComponentData, IEnableableComponent {
        public float3 StartPosition;
        public float3 EndPosition;
    }
}