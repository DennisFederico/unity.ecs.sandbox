using UnityEngine;
using Random = UnityEngine.Random;

namespace Crowds {
    public class CrowdMemberMono : MonoBehaviour {
        private Vector3 _targetPosition;
        private float _speed;

        private void Start() {
            _speed = Random.Range(1f, 3f);
        }
        
        private void Update() {
            Move(Time.deltaTime);
            TestReachedPosition();
        }

        private void Move(float deltaTime) {
            Vector3 direction = (_targetPosition - transform.position).normalized;
            transform.position += direction * (_speed * deltaTime);
        }

        private void TestReachedPosition() {
            float minDistance = 0.1f;
            if (Vector3.Distance(transform.position, _targetPosition) < minDistance) {
                _targetPosition = NewRandomPosition();
                _speed = Random.Range(1f, 3f);
            }
        }
        
        private Vector3 NewRandomPosition() {
            return new Vector3 {
                x = Random.Range(-25, 25f),
                y = 0f,
                z = Random.Range(-25f, 25f)
            };
        }
    }
}