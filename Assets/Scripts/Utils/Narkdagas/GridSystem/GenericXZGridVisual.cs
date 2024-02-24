using System;
using CodeMonkey.Utils;
using UnityEngine;

namespace Utils.Narkdagas.GridSystem {

    public class GenericXZGridVisual<TGridType> where TGridType : struct {

        private GenericXZGrid<TGridType> _grid;
        private Vector3 _originOffset;
        private Mesh _mesh;
        private Vector3 _quadSize;
        private Func<TGridType, float> _normalizeFunc;
        private bool _updateVisual;

        public GenericXZGridVisual(GenericXZGrid<TGridType> grid, Mesh mesh, Func<TGridType, float> normalizeFunc, Vector3 originOffset) {
            _grid = grid;
            _mesh = mesh;
            _quadSize = new Vector3(1, 1, 0) * _grid.CellSize;
            _normalizeFunc = normalizeFunc;
            _grid.GridValueChanged += GridValueChanged;
            _originOffset = originOffset;
            PaintVisual();
        }

        private void GridValueChanged(object sender, GenericXZGrid<TGridType>.OnGridValueChangedEventArgs eventArgs) {
            //TODO Only update the visual for the grid objects that changed in eventArgs
            _updateVisual = true;
        }

        public void LateUpdateVisual() {
            if (!_updateVisual) return;
            _updateVisual = false;
            PaintVisual();
        }

        protected void PaintVisual() {
            MeshUtils.CreateEmptyMeshArrays(
                _grid.Width * _grid.Height,
                out Vector3[] vertices,
                out Vector2[] uvs,
                out int[] triangles
            );

            for (int x = 0; x < _grid.Width; x++) {
                for (int y = 0; y < _grid.Height; y++) {
                    var index = _grid.GetFlatIndex(x, y);
                    var normalizedValue = _normalizeFunc(_grid.GetGridObject(x, y));
                    var uvValue = new Vector2(normalizedValue, 0f);
                    MeshUtils.AddToMeshArrays(
                        vertices,
                        uvs,
                        triangles,
                        index,
                        _grid.GetWorldPosition(x, y) - _originOffset, //Grid center
                        0,
                        _quadSize,
                        uvValue,
                        uvValue
                    );
                }
            }

            _mesh.vertices = vertices;
            _mesh.triangles = triangles;
            _mesh.uv = uvs;
        }
    }
}