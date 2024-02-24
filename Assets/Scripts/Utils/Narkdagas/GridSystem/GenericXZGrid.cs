using System;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace Utils.Narkdagas.GridSystem {

    public class GenericXZGrid<TGridType> where TGridType : struct {
        private readonly Vector3 _origin;
        private readonly TGridType[,] _gridArray;
        private readonly TextMesh[,] _debugTextArray;
        private int _width;
        private int _height;
        private float _cellSize;

        public class OnGridValueChangedEventArgs : EventArgs {
            public int X;
            public int Y;
        }

        public event EventHandler<OnGridValueChangedEventArgs> GridValueChanged;

        public int Width {
            get => _width;
            set => _width = value;
        }

        public int Height {
            get => _height;
            set => _height = value;
        }

        public float CellSize {
            get => _cellSize;
            set => _cellSize = value;
        }

        public GenericXZGrid(Vector3 origin, int width, int height, float cellSize, Func<int, int2, TGridType> createFunc, bool debug = false) {
            _origin = origin;
            _width = width;
            _height = height;
            _cellSize = cellSize;
            _gridArray = new TGridType[_width, _height];
            InitGrid(createFunc);
            if (debug) PaintDebugGrid();
        }

        //TODO Should we have the cell center WorldPosition as part of the GridCellInfo?
        //The internal grid array uses x and y as the first and second index, but the world position uses x and z
        private void InitGrid(Func<int, int2, TGridType> createFunc) {
            for (int x = 0; x < _width; x++) {
                for (int y = 0; y < _height; y++) {
                    _gridArray[x, y] = createFunc(GetFlatIndex(x, y), new int2(x, y));
                }
            }
        }

        public Vector3 GetWorldPosition(int2 pos, bool center = false) {
            return GetWorldPosition(pos.x, pos.y, center);
        }

        public Vector3 GetWorldPosition(int x, int y, bool center = false) {
            var position = _origin + (new Vector3(x, 0, y) * _cellSize);
            if (center) {
                return position + (new Vector3(1, 0, 1) * (_cellSize * .5f));
            }

            return position;
        }

        public int GetFlatIndex(int x, int y) => y * _width + x;

        public int GetFlatIndexSafe(int x, int y) {
            if (IsValidPosition(x, y)) {
                return GetFlatIndex(x, y);
            }

            return -1;
        }

        public void SetGridObject(int2 pos, TGridType gridObject) {
            SetGridObject(pos.x, pos.y, gridObject);
        }

        public void SetGridObject(int x, int y, TGridType gridObject) {
            if (!IsValidPosition(x, y)) return;
            _gridArray[x, y] = gridObject;
            TriggerGridValueChanged(x, y);
        }

        public void SetGridObject(Vector3 worldPosition, TGridType gridObject) {
            if (!TryGetXY(worldPosition, out var x, out var y)) return;
            _gridArray[x, y] = gridObject;
            TriggerGridValueChanged(x, y);
        }

        public bool TrySetGridObject(Vector3 worldPosition, TGridType gridObject) {
            if (!TryGetXY(worldPosition, out var x, out var y)) return false;
            _gridArray[x, y] = gridObject;
            TriggerGridValueChanged(x, y);
            return true;
        }

        public TGridType GetGridObject(int2 gridPos) {
            return GetGridObject(gridPos.x, gridPos.y);
        }

        public TGridType GetGridObject(int x, int y) {
            if (IsValidPosition(x, y)) {
                return _gridArray[x, y];
            }

            return default;
        }

        public TGridType GetGridObject(Vector3 worldPosition) {
            return TryGetXY(worldPosition, out var x, out var y) ? _gridArray[x, y] : default;
        }

        public bool TryGetGridObject(Vector3 worldPosition, out TGridType value) {
            if (TryGetXY(worldPosition, out var x, out var y)) {
                value = _gridArray[x, y];
                return true;
            }

            value = default;
            return false;
        }

        public bool GetCellGridWorldPositions(Vector3 worldPosition, out Vector3 cellWorldPosition, out int2 gridPosition, bool center = false) {
            if (TryGetXY(worldPosition, out gridPosition)) {
                cellWorldPosition = GetWorldPosition(gridPosition, center);
                return true;
            }

            cellWorldPosition = default;
            return false;
        }

        public bool TryGetXY(Vector3 worldPosition, out int x, out int y) {
            x = Mathf.FloorToInt((worldPosition.x - _origin.x) / _cellSize);
            y = Mathf.FloorToInt((worldPosition.z - _origin.z) / _cellSize);
            return IsValidPosition(x, y);
        }

        public bool TryGetXY(Vector3 worldPosition, out int2 coords) {
            var x = Mathf.FloorToInt((worldPosition.x - _origin.x) / _cellSize);
            var y = Mathf.FloorToInt((worldPosition.z - _origin.z) / _cellSize);
            coords = new int2(x, y);
            return IsValidPosition(x, y);
        }

        private void TriggerGridValueChanged(int x, int y) {
            GridValueChanged?.Invoke(this, new OnGridValueChangedEventArgs { X = x, Y = y });
        }

        private bool IsValidPosition(int x, int y) => x >= 0 && y >= 0 && x < _width && y < _height;

        public NativeArray<TGridType> GetGridAsArray(Allocator allocator) {
            var array = new NativeArray<TGridType>(_width * _height, allocator);
            for (int x = 0; x < _width; x++) {
                for (int y = 0; y < _height; y++) {
                    array[GetFlatIndex(x, y)] = _gridArray[x, y];
                }
            }

            return array;
        }

        public void PaintDebugGrid() {
            if (_gridArray == null) {
                return;
            }

            for (int x = 0; x < _gridArray.GetLength(0); x++) {
                for (int y = 0; y < _gridArray.GetLength(1); y++) {
                    Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x, y + 1), Color.green, 100f);
                    Debug.DrawLine(GetWorldPosition(x, y), GetWorldPosition(x + 1, y), Color.green, 100f);
                }
            }

            Debug.DrawLine(GetWorldPosition(0, _height), GetWorldPosition(_width, _height), Color.green, 100f);
            Debug.DrawLine(GetWorldPosition(_width, 0), GetWorldPosition(_width, _height), Color.green, 100f);
        }
    }
}