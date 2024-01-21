using Unity.Entities;

namespace Formations.Components {
    public struct TowerComponent : IComponentData {
        public Formation Formation;
        public int UnitCount;
        public float Radius;
    }
}