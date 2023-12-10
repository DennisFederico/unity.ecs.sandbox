using Unity.Entities;
using Unity.Mathematics;

namespace Towers.Components {
    public struct MoveComponent : IComponentData {
        public float3 TargetPosition;
        public float Speed;
        public int FormationIndex;
        public Formation Formation;
    }
}