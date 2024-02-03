using Unity.Entities;
using Unity.Mathematics;

namespace AStar.Components {
    public struct GridSingletonComponent : IComponentData {
        public float3 Origin;
        public int Width;
        public int Height;
        public int CellSize;
        
        public int GetIndex(int2 xy) => GetIndex(xy.x, xy.y);
        public int GetIndex(int x, int y) => y * Width + x;
        
        public bool IsValidPosition(int2 xy) => IsValidPosition(xy.x, xy.y);
        
        public bool IsValidPosition(int x, int y) => x >= 0 && y >= 0 && x < Width && y < Height;
        
        //return the grid center
        public float3 GetWorldPosition(int2 pos) => GetWorldPosition(pos.x, pos.y);
        public float3 GetWorldPosition(int x, int y) => new float3(x, y, 0) * CellSize + new float3(1, 1, 0) * (CellSize * .5f) + Origin;
    }
}