using Unity.Entities;

namespace Towers.Components {
    public struct TowerComponent : IComponentData {
        public Formation Formation;
        public int UnitCount;
        public float Radius;
    }
}