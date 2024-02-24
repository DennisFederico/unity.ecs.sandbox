using System;
using CodeMonkey.Utils;
using UnityEngine;

namespace Utils.Narkdagas.GridSystem {

    public class GenericXZGridDebugVisual<TGridType> where TGridType : struct {

        private GenericXZGrid<TGridType> _grid;
        private TextMesh[,] _debugText;
        private Func<TGridType, float> _normalizeFunc;

        public GenericXZGridDebugVisual(GenericXZGrid<TGridType> grid) {
            _grid = grid;
            _debugText = new TextMesh[_grid.Width, _grid.Height];
            _grid.GridValueChanged += GridValueChanged;
            PaintVisual();
        }

        private void GridValueChanged(object sender, GenericXZGrid<TGridType>.OnGridValueChangedEventArgs eventArgs) {
            // Debug.Log($"changed [{eventArgs.X},{eventArgs.Y}] to {_grid.GetGridObject(eventArgs.X, eventArgs.Y).ToString()}");
            _debugText[eventArgs.X, eventArgs.Y].text = _grid.GetGridObject(eventArgs.X, eventArgs.Y).ToString();
        }

        private void PaintVisual() {
            for (int x = 0; x < _grid.Width; x++) {
                for (int y = 0; y < _grid.Height; y++) {
                    var debugText = UtilsClass.CreateWorldText(_grid.GetGridObject(x, y).ToString(), null, _grid.GetWorldPosition(x, y, true), 8, Color.white, TextAnchor.MiddleCenter, TextAlignment.Center, 1);
                    debugText.transform.Rotate(Vector3.right, 90);
                    _debugText[x,y] = debugText; 
                }
            }
        }
    }
}