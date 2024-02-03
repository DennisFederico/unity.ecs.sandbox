using System;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Utils.Narkdagas.PathFinding {

    [BurstCompile]
    [Serializable]
    [InternalBufferCapacity(0)]
    public struct PathNode : IPathNode, IBufferElementData {
        public int Index { get; set; }
        public int2 XY { get; set; }
        public bool IsWalkable { get; set; }
        public int GCost { get; set; }
        public int HCost { get; set; }
        public int FCost => GCost + HCost;
        
        //TODO ParentXY!?
        public int ParentIndex { get; set; }
        
        public void ResetCosts(int hCost)  {
            GCost = int.MaxValue;
            HCost = hCost;
            ParentIndex = -1;
        }
        
        public override string ToString() {
            // return $"{Index} [{GridPosition.x}, {GridPosition.y}]\n[{GCost},{HCost},{FCost}]";
            return $"{Index} [{XY.x}, {XY.y}]";
        }
    }
    
    public interface IPathNode : IGridNode {
        
        int GCost { get; set; }
        int HCost { get; set; }
        int FCost { get; }
        int ParentIndex { get; set; }
        void ResetCosts(int hCost);
    }

    [BurstCompile]
    public struct GridNode : IGridNode {
        public int Index { get; set; }
        public int2 XY { get; set; }
        public bool IsWalkable { get; set; }
        
        public override string ToString() {
            return $"{Index} [{XY.x}, {XY.y}]\n[{IsWalkable}]";
        }
    }

    public interface IGridNode {
        int Index { get; set; }
        int2 XY { get; set; }
        bool IsWalkable { get; set; }
        
        void SetIsWalkable(bool isWalkable) {
            IsWalkable = isWalkable;
        }
    }
}