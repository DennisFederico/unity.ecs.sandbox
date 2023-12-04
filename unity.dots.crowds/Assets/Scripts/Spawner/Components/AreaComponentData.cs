using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Spawner.Components {
    [Serializable]
    public struct AreaComponentData : IComponentData {
        public float3 area;
    }
}