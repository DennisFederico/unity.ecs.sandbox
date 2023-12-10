using Unity.Entities;

namespace Towers.Components {
    public struct FormationComponent : IComponentData {
        public Formation Value;
        public int Index;
    }
}