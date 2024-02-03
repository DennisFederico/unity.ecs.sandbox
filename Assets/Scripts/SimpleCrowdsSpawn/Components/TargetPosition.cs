using Unity.Entities;
using Unity.Mathematics;

namespace SimpleCrowdsSpawn.Components {
    public struct TargetPosition : IComponentData {
        public float3 Value;
    }
}