using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace Utils.Narkdagas.PathFinding {

    [BurstCompile]
    public struct PathfindingJob : IJob {

        private const int DiagonalCost = 14;
        private const int StraightCost = 10;

        [ReadOnly]
        public NativeArray<PathNode> GridArray;
        [ReadOnly]
        public int2 GridSize;
        [ReadOnly]
        public int2 FromPosition;
        [ReadOnly]
        public int2 ToPosition;
        [NativeDisableContainerSafetyRestriction]
        public NativeList<int2> ResultPath;

        public void Execute() {
            FindPath(FromPosition, ToPosition, GridSize);
        }

        private void FindPath(in int2 fromPosition, in int2 toPosition, in int2 gridSize) {
            if ((fromPosition == toPosition) is { x: true, y: true }) return;
            var localGrid = InitLocalGrid(GridArray, GridSize, toPosition, Allocator.Temp);
            
            //Initialize the algorithm
            var offsets = new NativeArray<int2>(8, Allocator.Temp);
            offsets[0] = new int2(-1, 1); //Top Left
            offsets[1] = new int2(0, 1); //Top
            offsets[2] = new int2(1, 1); //Top Right
            offsets[3] = new int2(1, 0); //Right
            offsets[4] = new int2(1, -1); //Bottom Right
            offsets[5] = new int2(0, -1); //Bottom
            offsets[6] = new int2(-1, -1); //Bottom Left
            offsets[7] = new int2(-1, 0); //Left
            
            var openList = new NativeList<int>(Allocator.Temp);
            var closedList = new NativeList<int>(Allocator.Temp);
            
            var startNodeIndex = PathNodeIndex(fromPosition, gridSize);
            var startNode = localGrid[startNodeIndex];
            startNode.GCost = 0;
            localGrid[startNodeIndex] = startNode;
            
            openList.Add(startNodeIndex);
            
            while (openList.Length > 0) {
                if (!TryGetNodeIndexWithLowestFCost(openList, localGrid, out var openListIndex)) continue;
                var currentNodeIndex = openList[openListIndex];
                var currentNode = localGrid[currentNodeIndex];
                if (currentNode.XY.Equals(toPosition)) {
                    //We found the path
                    break;
                }
            
                //Remove the current node from the open list
                openList.RemoveAtSwapBack(openListIndex);
            
                //Add the current node to the closed list
                closedList.Add(currentNodeIndex);
            
                //Loop through the neighbors of the current node
                foreach (var offset in offsets) {
                    var neighborGridPosition = currentNode.XY + offset;
                    if (!IsPositionInsideGrid(neighborGridPosition, gridSize)) {
                        //This neighbor is outside the grid
                        continue;
                    }
            
                    var neighborNodeIndex = PathNodeIndex(neighborGridPosition, gridSize);
                    if (closedList.Contains(neighborNodeIndex)) {
                        //This neighbor is already in the closed list
                        continue;
                    }
            
                    var neighborNode = localGrid[neighborNodeIndex];
                    if (!neighborNode.IsWalkable) {
                        //This neighbor is not walkable
                        continue;
                    }
            
                    var tentativeGCost = currentNode.GCost + DistanceCost(currentNode.XY, neighborNode.XY);
                    if (tentativeGCost < neighborNode.GCost) {
                        //This is a better path to the neighbor
                        neighborNode.GCost = tentativeGCost;
                        neighborNode.ParentIndex = currentNodeIndex;
                        localGrid[neighborNodeIndex] = neighborNode;
                        if (!openList.Contains(neighborNodeIndex)) {
                            //Add the neighbor to the open list
                            openList.Add(neighborNodeIndex);
                        }
                    }
                }
            }
            
            //Do we have a path?
            if (localGrid[PathNodeIndex(toPosition, gridSize)].ParentIndex != -1) {
                BacktrackPathFromEndNode(PathNodeIndex(toPosition, gridSize), localGrid, ResultPath);
            }
            
            // offsets.Dispose();
            // openList.Dispose();
            // closedList.Dispose();
            // localGrid.Dispose();
        }

        //Flattens the index of a 2D array into a 1D array
        private static int PathNodeIndex(in int2 gridPosition, in int2 gridSize) => gridPosition.x + (gridPosition.y * gridSize.x);

        private static int DistanceCost(in int2 a, in int2 b) {
            var xDistance = math.abs(a.x - b.x);
            var yDistance = math.abs(a.y - b.y);

            //This is the amount we would move in a straight line
            var straight = math.abs(xDistance - yDistance);
            //This is the amount we would move diagonally
            var diagonally = math.min(xDistance, yDistance);

            return DiagonalCost * diagonally + StraightCost * straight;
        }

        private static bool IsPositionInsideGrid(in int2 gridPosition, in int2 gridSize) =>
            gridPosition is { x: >= 0, y: >= 0 } && gridPosition.x < gridSize.x && gridPosition.y < gridSize.y;

        private static bool TryGetNodeIndexWithLowestFCost(in NativeList<int> openList, in NativeArray<PathNode> pathNodes, out int openListIndex) {
            var lowestCost = int.MaxValue;
            var lowestCostIndex = -1;
            for (var i = 0; i < openList.Length; i++) {
                var pathNode = pathNodes[openList[i]];
                if (pathNode.FCost < lowestCost) {
                    lowestCost = pathNode.FCost;
                    lowestCostIndex = i;
                }
            }

            openListIndex = lowestCostIndex;
            return openListIndex >= 0;
        }

        private static void BacktrackPathFromEndNode(int endNodeIndex, NativeArray<PathNode> pathNodes, NativeList<int2> resultPath) {
            var path = new NativeList<int2>(Allocator.Temp);
            var nextNodeIndex = endNodeIndex;
        
            while (nextNodeIndex != -1) {
                var currentNode = pathNodes[nextNodeIndex];
                path.Add(currentNode.XY);
                nextNodeIndex = currentNode.ParentIndex;
            }
        
            resultPath.ResizeUninitialized(path.Length);
            int reverseIndex = 0;
            for (int index = path.Length - 1; index >= 0; index--) {
                resultPath[reverseIndex++] = path[index];
            }
        
            // path.Dispose();
            // return !resultPath.IsEmpty;
        }

        private static NativeArray<PathNode> InitLocalGrid(in NativeArray<PathNode> walkableFlags, in int2 gridSize, int2 toPosition, in Allocator allocator) {
            var gridArray = new NativeArray<PathNode>(gridSize.x * gridSize.y, allocator);
            //Initialize the PathNodes
            for (var x = 0; x < gridSize.x; x++) {
                for (var y = 0; y < gridSize.y; y++) {
                    var gridPosition = new int2(x, y);
                    var gridIndex = PathNodeIndex(gridPosition, gridSize);
                    var pathNode = new PathNode {
                        Index = gridIndex,
                        XY = gridPosition,
                        IsWalkable = walkableFlags[gridIndex].IsWalkable,
                        GCost = int.MaxValue,
                        HCost = DistanceCost(gridPosition, toPosition),
                        ParentIndex = -1
                    };
                    gridArray[pathNode.Index] = pathNode;
                }
            }

            return gridArray;
        }
    }
}