using CodeMonkey.Utils;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Utils.Narkdagas.PathFinding {
    public class PathfindingDots : MonoBehaviour {
        private const int DiagonalCost = 14;
        private const int StraightCost = 10;

        private struct PathNode {
            public int Index;
            public int2 GridPosition;
            public bool IsWalkable;
            public int GCost;
            public int HCost;
            public int FCost => GCost + HCost;
            public int ParentIndex;
        }

        [BurstCompile]
        private struct FindPathJob : IJob {
            
            public int2 FromPosition;
            public int2 ToPosition;
            public int2 GridSize;
            // public NativeArray<int2> resultPath;

            public void Execute() {
                FindPath(FromPosition, ToPosition, GridSize);
            }

            private void FindPath(int2 fromPosition, int2 toPosition, int2 gridSize) {
                //Create a Native (thread-safe) "Flat" Array of PathNodes
                var pathNodes = new NativeArray<PathNode>(gridSize.x * gridSize.y, Allocator.Temp);

                //Initialize the PathNodes
                for (var x = 0; x < gridSize.x; x++) {
                    for (var y = 0; y < gridSize.y; y++) {
                        var gridPosition = new int2(x, y);
                        var pathNode = new PathNode {
                            Index = PathNodeIndex(gridPosition, gridSize),
                            GridPosition = gridPosition,
                            IsWalkable = true,
                            GCost = int.MaxValue,
                            HCost = DistanceCost(gridPosition, toPosition),
                            ParentIndex = -1
                        };
                        pathNodes[pathNode.Index] = pathNode;
                    }
                }

                PlaceWalls(gridSize, pathNodes);

                //Initialize the algorithm
                var offsets = new NativeArray<int2>(8, Allocator.Temp);
                offsets[0] = new(-1, 1); //Top Left
                offsets[1] = new(0, 1); //Top
                offsets[2] = new(1, 1); //Top Right
                offsets[3] = new(1, 0); //Right
                offsets[4] = new(1, -1); //Bottom Right
                offsets[5] = new(0, -1); //Bottom
                offsets[6] = new(-1, -1); //Bottom Left
                offsets[7] = new(-1, 0); //Left
                
                var openList = new NativeList<int>(Allocator.Temp);
                var closedList = new NativeList<int>(Allocator.Temp);

                var startNodeIndex = PathNodeIndex(fromPosition, gridSize);
                var startNode = pathNodes[startNodeIndex];
                startNode.GCost = 0;
                pathNodes[startNodeIndex] = startNode;

                openList.Add(startNodeIndex);

                while (openList.Length > 0) {
                    if (TryGetNodeIndexWithLowestFCost(openList, pathNodes, out var openListIndex)) {
                        var currentNodeIndex = openList[openListIndex];
                        var currentNode = pathNodes[currentNodeIndex];
                        if (currentNode.GridPosition.Equals(toPosition)) {
                            //We found the path
                            break;
                        }

                        //Remove the current node from the open list
                        openList.RemoveAtSwapBack(openListIndex);

                        //Add the current node to the closed list
                        closedList.Add(currentNodeIndex);

                        //Loop through the neighbors of the current node
                        foreach (var offset in offsets) {
                            var neighborGridPosition = currentNode.GridPosition + offset;
                            if (!IsPositionInsideGrid(neighborGridPosition, gridSize)) {
                                //This neighbor is outside the grid
                                continue;
                            }

                            var neighborNodeIndex = PathNodeIndex(neighborGridPosition, gridSize);
                            if (closedList.Contains(neighborNodeIndex)) {
                                //This neighbor is already in the closed list
                                continue;
                            }

                            var neighborNode = pathNodes[neighborNodeIndex];
                            if (!neighborNode.IsWalkable) {
                                //This neighbor is not walkable
                                continue;
                            }

                            var tentativeGCost = currentNode.GCost + DistanceCost(currentNode.GridPosition, neighborNode.GridPosition);
                            if (tentativeGCost < neighborNode.GCost) {
                                //This is a better path to the neighbor
                                neighborNode.GCost = tentativeGCost;
                                neighborNode.ParentIndex = currentNodeIndex;
                                pathNodes[neighborNodeIndex] = neighborNode;
                                if (!openList.Contains(neighborNodeIndex)) {
                                    //Add the neighbor to the open list
                                    openList.Add(neighborNodeIndex);
                                }
                            }
                        }
                    }
                }

                //We have either found the path or there is no path
                if (pathNodes[PathNodeIndex(toPosition, gridSize)].ParentIndex == -1) {
                    //There is no path
                    Debug.Log("No Path");
                } else {
                    //There is a path
                    var backtrackPath = BacktrackPathFromEndNode(PathNodeIndex(toPosition, gridSize), pathNodes);
                    var nativeArray = backtrackPath.ToArray(Allocator.Temp);
                    // string result = "";
                    // foreach (var pos in nativeArray) {
                    //     result += pos + " ";
                    // }
                    // Debug.Log($"Path: {result}");
                    nativeArray.Dispose();
                    backtrackPath.Dispose();
                }

                pathNodes.Dispose();
                offsets.Dispose();
                openList.Dispose();
                closedList.Dispose();
            }

            //Flattens the index of a 2D array into a 1D array
            private int PathNodeIndex(int2 gridPosition, int2 gridSize) => gridPosition.x + (gridPosition.y * gridSize.x);

            private int DistanceCost(int2 a, int2 b) {
                var xDistance = math.abs(a.x - b.x);
                var yDistance = math.abs(a.y - b.y);

                //This is the amount we would move in a straight line
                var straight = math.abs(xDistance - yDistance);
                //This is the amount we would move diagonally
                var diagonally = math.min(xDistance, yDistance);

                return DiagonalCost * diagonally + StraightCost * straight;
            }

            private bool TryGetNodeIndexWithLowestFCost(NativeList<int> openList, NativeArray<PathNode> pathNodes, out int openListIndex) {
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

            private bool IsPositionInsideGrid(int2 gridPosition, int2 gridSize) =>
                gridPosition is { x: >= 0, y: >= 0 } && gridPosition.x < gridSize.x && gridPosition.y < gridSize.y;

            private NativeList<int2> BacktrackPathFromEndNode(int endNodeIndex, NativeArray<PathNode> pathNodes) {
                var path = new NativeList<int2>(Allocator.Temp);
                var currentNodeIndex = endNodeIndex;
                while (currentNodeIndex != -1) {
                    var currentNode = pathNodes[currentNodeIndex];
                    path.Add(currentNode.GridPosition);
                    currentNodeIndex = currentNode.ParentIndex;
                }

                return path;
            }

            private void PlaceWalls(int2 gridSize, NativeArray<PathNode> pathNodes) {
                //NON-WALKABLE NODES
                var node = pathNodes[PathNodeIndex(new int2(1, 0), gridSize)];
                node.IsWalkable = false;
                pathNodes[PathNodeIndex(new int2(1, 0), gridSize)] = node;

                node = pathNodes[PathNodeIndex(new int2(1, 1), gridSize)];
                node.IsWalkable = false;
                pathNodes[PathNodeIndex(new int2(1, 1), gridSize)] = node;

                node = pathNodes[PathNodeIndex(new int2(1, 2), gridSize)];
                node.IsWalkable = false;
                pathNodes[PathNodeIndex(new int2(1, 2), gridSize)] = node;

                node = pathNodes[PathNodeIndex(new int2(2, 2), gridSize)];
                node.IsWalkable = false;
                pathNodes[PathNodeIndex(new int2(2, 2), gridSize)] = node;
            }
        }

        private void Start() {
            FunctionPeriodic.Create(() => {
                float startTime = Time.realtimeSinceStartup;
                //FindPath(new int2(0, 0), new int2(18, 13), new int2(20, 20));
                
                int numJobs = 5;
                var jobs = new NativeArray<JobHandle>(numJobs, Allocator.TempJob);
                for (int i = 0; i < 5; i++) {
                    FindPathJob job = new FindPathJob {
                        FromPosition = new int2(0, 0),
                        ToPosition = new int2(18, 13),
                        GridSize = new int2(20, 20)
                    };
                    jobs[i] = job.Schedule();
                }
                var combinedDependencies = JobHandle.CombineDependencies(jobs);
                jobs.Dispose(combinedDependencies);
                combinedDependencies.Complete();
                
                Debug.Log($"Time: {(Time.realtimeSinceStartup - startTime) * 1000f} ms");
            }, 1f);

            // var gridSize = new int2(4, 4);
            // var fromPosition = new int2(0, 0);
            // var toPosition = new int2(3, 1);
            // FindPath(fromPosition, toPosition, gridSize);
        }
    }
}