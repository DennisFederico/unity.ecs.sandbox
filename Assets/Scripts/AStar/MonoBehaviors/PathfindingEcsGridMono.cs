using System.Collections.Generic;
using AStar.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Utils.Narkdagas.GridSystem;
using Utils.Narkdagas.PathFinding;
using Utils.Narkdagas.PathFinding.MonoTester;
using Random = Unity.Mathematics.Random;

namespace AStar.MonoBehaviors {

    public class PathfindingEcsGridMono : MonoBehaviour {

        [SerializeField] private int width;
        [SerializeField] private int height;
        [SerializeField] private int cellSize;
        [SerializeField] private Material gradientMaterial;
        [SerializeField] private int armySize;

        // public event EventHandler<NewGridPathRequestEvent> NewGridPathRequestEvent;
        
        private Camera _camera;
        private Mesh _mesh;
        private GenericSimpleGrid<PathNode> _grid;
        private GenericSimpleGridVisual<PathNode> _gridVisual;
        private PathfindingMovement _player;
        private List<PathfindingMovement> _army;
        private Random _random;
        
        private World _world; //Used to inject the Grid into the ECS system
        private Entity _theGrid;
        private Entity _createFollowerBufferEntity;
        private Entity _removeFollowerBufferEntity;

        private void Awake() {
            _camera = Camera.main;
            _mesh = new Mesh();
            GetComponent<MeshRenderer>().material = gradientMaterial;
            GetComponent<MeshFilter>().mesh = _mesh;
            _random = Random.CreateFromIndex((uint)UnityEngine.Random.Range(0, int.MaxValue));
            
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
        }
        
        private void OnEnable() {
            _camera = Camera.main;
            _grid.GridValueChanged += GridOnGridValueChanged;
            _world = World.DefaultGameObjectInjectionWorld;
            if (_world.IsCreated && !_world.EntityManager.Exists(_createFollowerBufferEntity)) {
                _createFollowerBufferEntity = _world.EntityManager.CreateSingletonBuffer<CreateNewPathFollowerRequest>("CreateNewPathFollowerBuffer");
            }
            if (_world.IsCreated && !_world.EntityManager.Exists(_removeFollowerBufferEntity)) {
                _removeFollowerBufferEntity = _world.EntityManager.CreateSingletonBuffer<RemovePathFollowerRequest>("RemovePathFollowerBuffer");
            }
        }
        
        private void OnDisable() {
            _grid.GridValueChanged -= GridOnGridValueChanged;
            if (_world.IsCreated && _world.EntityManager.Exists(_createFollowerBufferEntity)) {
                _world.EntityManager.DestroyEntity(_createFollowerBufferEntity);
            }
            if (_world.IsCreated && _world.EntityManager.Exists(_removeFollowerBufferEntity)) {
                _world.EntityManager.DestroyEntity(_removeFollowerBufferEntity);
            }
        }
        
        private void GridOnGridValueChanged(object sender, OnGridValueChangedEventArgs e) {
            //Notify the system about changes in the grid
            if (_world.IsCreated && _world.EntityManager.Exists(_theGrid)) {
                var ecsGrid = _world.EntityManager.GetBuffer<PathNode>(_theGrid);
                ecsGrid.CopyFrom(_grid.GetGridAsArray(Allocator.Temp));
                ecsGrid.TrimExcess();
            } 
        }
        
        private void Start() {
            _grid.PaintDebugGrid();
            InjectGridIntoEcsSystem();
        }

        private void InjectGridIntoEcsSystem() {
            if (_world.IsCreated) {
                _theGrid = _world.EntityManager.CreateSingleton(new GridSingletonComponent {
                    Width = width,
                    Height = height,
                    CellSize = cellSize,
                    Origin = transform.position
                },"TheGrid");
                
                _world.EntityManager.AddComponent<PathNode>(_theGrid);
                var ecsGrid = _world.EntityManager.GetBuffer<PathNode>(_theGrid);
                ecsGrid.AddRange(_grid.GetGridAsArray(Allocator.Temp));
            } 
        }

        private Vector3 _startDragPosition;

        private void Update() {
            
            //Right clicks to toggle walkable grid
            if (Input.GetMouseButtonDown(1)) {
                if (!_grid.TryGetXY(_camera.ScreenToWorldPoint(Input.mousePosition), out var x, out var y)) return;
                var node = _grid.GetGridObject(x, y);
                node.IsWalkable = !node.IsWalkable;
                _grid.SetGridObject(x, y, node);
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
                    if (_world.IsCreated && _createFollowerBufferEntity != Entity.Null) {
                        var buffer = _world.EntityManager.GetBuffer<CreateNewPathFollowerRequest>(_createFollowerBufferEntity);
                        buffer.Add(new CreateNewPathFollowerRequest {
                            StartPosition = _grid.GetWorldPosition(startPos),
                            EndPosition = _grid.GetWorldPosition(endPos),
                            TimeToLive = 6f
                        });
                    }
                }
            }
            
            if (Input.GetKeyDown(KeyCode.KeypadPlus)) {
                SpawnArmy();
            }
            
            if (Input.GetKeyDown(KeyCode.KeypadMinus)) {
                DeSpawnArmy();
            }
        }

        private void LateUpdate() {
            _gridVisual.LateUpdateVisual();
        }

        public void SpawnArmy() {
            var startTime = Time.realtimeSinceStartup;
            if (_world.IsCreated && _createFollowerBufferEntity != Entity.Null) {
                var buffer = _world.EntityManager.GetBuffer<CreateNewPathFollowerRequest>(_createFollowerBufferEntity);
                for (int i = 0; i < armySize; i++) {
                    buffer.Add(new CreateNewPathFollowerRequest {
                        StartPosition = _grid.GetWorldPosition(GetRandomWalkablePosition()),
                        EndPosition = _grid.GetWorldPosition(GetRandomWalkablePosition())
                    });
                }
            }
            Debug.Log($"Request to spawn a platoon of {armySize} in {Time.realtimeSinceStartup - startTime}");
        }
        
        public void DeSpawnArmy() {
            var startTime = Time.realtimeSinceStartup;
            if (_world.IsCreated && _removeFollowerBufferEntity != Entity.Null) {
                var buffer = _world.EntityManager.GetBuffer<RemovePathFollowerRequest>(_removeFollowerBufferEntity);
                buffer.Add(new RemovePathFollowerRequest() {
                        Value = armySize
                });
            }
            Debug.Log($"Request to spawn a platoon of {armySize} in {Time.realtimeSinceStartup - startTime}");
        }
        
        private int2 GetRandomWalkablePosition() {
            var randomPosition = _random.NextInt2(int2.zero, new int2(width, height));
            if (_grid.GetGridObject(randomPosition.x, randomPosition.y).IsWalkable) {
                return randomPosition;
            }
            return GetRandomWalkablePosition();
        }
    }
}