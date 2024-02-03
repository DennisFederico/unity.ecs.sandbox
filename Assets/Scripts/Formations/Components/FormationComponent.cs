using Unity.Entities;

namespace Formations.Components {
    public struct FormationComponent : IComponentData {
        public Formation Value;
        public int Index;
    }
}