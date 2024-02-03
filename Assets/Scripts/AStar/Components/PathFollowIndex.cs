using Unity.Entities;

namespace AStar.Components {
    public struct PathFollowIndex : IComponentData, IEnableableComponent {
        public int Value;
    }
}