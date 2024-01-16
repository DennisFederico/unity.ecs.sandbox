using System;
using UnityEngine;

namespace Utils.Narkdagas.PathFinding.MonoTester {
    public class PathfindingMovement : MonoBehaviour {

        [SerializeField] private float speed = 10f;
        [SerializeField] private float distance = .5f;
        private Vector3[] _path;
        private int _pathIndex;
        private bool _pathRequested;
        private EventHandler<NewGridPathRequestEvent> _requestPathEventHandler;

        public void SetPath(Vector3[] path, EventHandler<NewGridPathRequestEvent> requestPathEventHandler) {
            _requestPathEventHandler = requestPathEventHandler;
            SetPath(path);
        }

        public void SetPath(Vector3[] path) {
            _pathRequested = false;
            _path = path;
            _pathIndex = 0;
            if (path is { Length: > 0 }) transform.position = path[0];
        }

        private void Update() {
            if (_path is { Length: > 0 } && _pathIndex < _path.Length) {
                var currentPosition = transform.position;
                var targetPosition = _path[_pathIndex];
                var newPosition = Vector3.MoveTowards(currentPosition, targetPosition, speed * Time.deltaTime);
                transform.position = newPosition;
                if (Vector3.Distance(newPosition, targetPosition) < distance) {
                    _pathIndex++;
                }
            } else {
                StopMoving();
            }
        }

        private void StopMoving() {
            _path = null;
            _pathRequested = true;
            if (!_pathRequested && _requestPathEventHandler == null) return;
            _pathRequested = true;
            _requestPathEventHandler?.Invoke(gameObject, new NewGridPathRequestEvent() {
                CurrentPosition = transform.position
            });
        }
    }
}