using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using Utils.Narkdagas.GridSystem;
using Utils.Narkdagas.PathFinding;
using Utils.Narkdagas.PathFinding.MonoTester;
using Random = Unity.Mathematics.Random;

namespace AStar.MonoBehaviors {

    //TODO WAIT FOR THE JOB TO COMPLETE ON A QUEUE AT FIXED UPDATE
    public class PathfindingGridMono : MonoBehaviour {

        [SerializeField] private int width;
        [SerializeField] private int height;
        [SerializeField] private float cellSize;
        [SerializeField] private Material gradientMaterial;
        [SerializeField] private PathfindingMovement prefab;
        [SerializeField] private int armySize;

        public event EventHandler<NewGridPathRequestEvent> NewGridPathRequestEvent;

        private Camera _camera;
        private Mesh _mesh;
        private GenericSimpleGrid<PathNode> _grid;
        private GenericSimpleGridVisual<PathNode> _gridVisual;
        private PathfindingMovement _player;
        private List<PathfindingMovement> _army;
        private Random _random;

        private void OnEnable() {
            NewGridPathRequestEvent += OnNewGridPathRequestEvent;
        }

        private void OnDisable() {
            NewGridPathRequestEvent -= OnNewGridPathRequestEvent;
        }

        private void OnNewGridPathRequestEvent(object sender, NewGridPathRequestEvent e) {
            if (!_grid.TryGetXY(e.CurrentPosition, out var x, out var y)) return;
            var targetGameObject = ((GameObject)sender).GetComponent<PathfindingMovement>();

            var endPos = _random.NextInt2(int2.zero, new int2(width, height));
            //TODO QUEUE THE WAIT FOR THE JOB TO COMPLETE ON A QUEUE AT FIXED UPDATE

            var gridAsArray = _grid.GetGridAsArray(Allocator.TempJob);
            var resultPath = new NativeList<int2>(Allocator.TempJob);
            var jobHandle = new PathfindingJob() {
                GridArray = gridAsArray,
                GridSize = new int2(width, height),
                FromPosition = new int2(x, y),
                ToPosition = endPos,
                ResultPath = resultPath
            }.Schedule();
            jobHandle.Complete();

            if (jobHandle.IsCompleted) {
                var path3 = TransformPath(resultPath);
                targetGameObject.SetPath(path3);
                resultPath.Dispose();
                gridAsArray.Dispose();
            }
        }

        private void Start() {
            _camera = Camera.main;
            _mesh = new Mesh();
            GetComponent<MeshRenderer>().material = gradientMaterial;
            GetComponent<MeshFilter>().mesh = _mesh;
            _random = Random.CreateFromIndex((uint)UnityEngine.Random.Range(0, int.MaxValue));
            _army = new List<PathfindingMovement>();

            var originOffset = transform.position;
            _grid = new GenericSimpleGrid<PathNode>(originOffset, width, height, cellSize,
                (index, gridPos) => new PathNode {
                    Index = index,
                    XY = gridPos,
                    IsWalkable = true
                });
            _gridVisual = new GenericSimpleGridVisual<PathNode>(_grid, _mesh, (node) => {
                if (!node.IsWalkable) return 0f;
                if (node.ParentIndex != -1) return .5f;
                return 0.25f;
            }, originOffset);
            // _grid.PaintDebugGrid();
        }

        private Vector3 _startDragPosition;

        private void Update() {
            if (Input.GetKeyDown(KeyCode.D)) {
                int numJobs = 500;
                // var startTime = Time.realtimeSinceStartup;
                // Debug.Log($"Start {numJobs} jobs at {startTime}");
                var gridAsArray = _grid.GetGridAsArray(Allocator.TempJob);

                var results = new NativeArray<NativeList<int2>>(numJobs, Allocator.TempJob);
                var handlers = new NativeArray<JobHandle>(numJobs, Allocator.TempJob);
                for (int i = 0; i < numJobs; i++) {
                    results[i] = new NativeList<int2>(numJobs, Allocator.TempJob);
                    var startPos = _random.NextInt2(int2.zero, new int2(width / 4, height / 4));
                    var endPos = _random.NextInt2(new int2(width / 2, height / 2), new int2(width - 1, height - 1));
                    var jobHandle = new PathfindingJob() {
                        GridArray = gridAsArray,
                        GridSize = new int2(width, height),
                        FromPosition = startPos,
                        ToPosition = endPos,
                        ResultPath = results[i]
                    }.Schedule();
                    handlers[i] = jobHandle;
                }

                JobHandle.CompleteAll(handlers);
                foreach (var result in results) {
                    result.Dispose();
                }

                results.Dispose();
                handlers.Dispose();
                gridAsArray.Dispose();

                // var endTime = Time.realtimeSinceStartup;
                // Debug.Log($"End {numJobs} jobs at {endTime} in {endTime - startTime}s");
            }

            if (Input.GetMouseButtonDown(0)) {
                _startDragPosition = _camera.ScreenToWorldPoint(Input.mousePosition);
            }

            if (Input.GetMouseButtonUp(0)) {
                var endDragPosition = _camera.ScreenToWorldPoint(Input.mousePosition);

                if (_grid.TryGetXY(_startDragPosition, out var startPos) && _grid.TryGetXY(endDragPosition, out var endPos)) {
                    if (math.distance(startPos, endPos) < 1) {
                        startPos = int2.zero;
                    }

                    var gridAsArray = _grid.GetGridAsArray(Allocator.TempJob);
                    var resultPath = new NativeList<int2>(Allocator.TempJob);
                    var jobHandle = new PathfindingJob() {
                        GridArray = gridAsArray,
                        GridSize = new int2(width, height),
                        FromPosition = startPos,
                        ToPosition = endPos,
                        ResultPath = resultPath
                    }.Schedule();
                    jobHandle.Complete();

                    if (jobHandle.IsCompleted) {
                        // DebugPath(resultPath.AsArray().ToArray());
                        var path3 = TransformPath(resultPath);
                        if (!_player) {
                            _player = Instantiate(prefab);
                        }

                        _player.SetPath(path3);
                        resultPath.Dispose();
                        gridAsArray.Dispose();
                    }
                }
            }

            if (Input.GetMouseButtonDown(1)) {
                if (!_grid.TryGetXY(_camera.ScreenToWorldPoint(Input.mousePosition), out var x, out var y)) return;
                var node = _grid.GetGridObject(x, y);
                node.IsWalkable = !node.IsWalkable;
                _grid.SetGridObject(x, y, node);
            }

            if (Input.GetKeyDown(KeyCode.A)) {
                // var startTime = Time.realtimeSinceStartup;
                // Debug.Log($"Building an platoon of {armySize} jobs at {startTime}");
            
                var platoon = new PathfindingMovement[armySize];
                var gridAsArray = _grid.GetGridAsArray(Allocator.TempJob);
                var results = new NativeArray<NativeList<int2>>(armySize, Allocator.TempJob);
                var handlers = new NativeArray<JobHandle>(armySize, Allocator.TempJob);
                var random = Random.CreateFromIndex((uint)Time.frameCount);
                for (int i = 0; i < armySize; i++) {
                    platoon[i] = Instantiate(prefab);
                    results[i] = new NativeList<int2>(armySize, Allocator.TempJob);
                    var startPos = random.NextInt2(new int2(0, 0), new int2(width - 1, height - 1));
                    var endPos = random.NextInt2(new int2(0, 0), new int2(width - 1, height - 1));
                    var jobHandle = new PathfindingJob() {
                        GridArray = gridAsArray,
                        GridSize = new int2(width, height),
                        FromPosition = startPos,
                        ToPosition = endPos,
                        ResultPath = results[i]
                    }.Schedule();
                    handlers[i] = jobHandle;
                }

                JobHandle.CompleteAll(handlers);
                for (int i = 0; i < armySize; i++) {
                    var path3 = TransformPath(results[i]);
                    results[i].Dispose();
                    platoon[i].SetPath(path3, this.NewGridPathRequestEvent);
                }
                
                results.Dispose();
                handlers.Dispose();
                gridAsArray.Dispose();
                _army.AddRange(platoon);
                
                // var endTime = Time.realtimeSinceStartup;
                // Debug.Log($"Added {armySize} to the army for {_army.Count} it total - at {endTime} in {endTime - startTime}s");
            }
        }

        private Vector3[] TransformPath(NativeList<int2> path) {
            var path3 = new Vector3[path.Length];
            int index = 0;
            foreach (var step in path) {
                path3[index++] = _grid.GetWorldPosition(step.x, step.y);
            }
            return path3;
        }

        private void LateUpdate() {
            _gridVisual.LateUpdateVisual();
        }

        // private void DebugPath(int2[] path) {
        //     var offset = new Vector3(cellSize / 2, cellSize / 2, 0);
        //     for (int i = 0; i < path.Length - 1; i++) {
        //         Debug.DrawLine(
        //             _grid.GetWorldPosition(path[i].x, path[i].y) + offset,
        //             _grid.GetWorldPosition(path[i + 1].x, path[i + 1].y) + offset,
        //             Color.red, 15f
        //         );
        //     }
        // }
    }

}