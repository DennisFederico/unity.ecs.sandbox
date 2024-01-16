using Unity.Entities;
using Unity.Mathematics;

namespace AStar.Components {
    public struct GridSingletonComponent : IComponentData {
        public float3 Origin;
        public int Width;
        public int Height;
        public int CellSize;
    }
}