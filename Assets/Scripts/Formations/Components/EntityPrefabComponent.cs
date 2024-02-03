using Unity.Entities;
using Unity.Mathematics;

namespace Formations.Components {
    public struct EntityPrefabComponent : IComponentData {
        public Entity Value;
        public float4 Color;
    }
}