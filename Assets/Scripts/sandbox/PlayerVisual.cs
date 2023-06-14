using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

namespace sandbox {
    public class PlayerVisual : MonoBehaviour {

        private Entity _targetEntity;

        private void LateUpdate() {
            if (Input.GetKeyDown(KeyCode.Space)) {
                _targetEntity = GetRandomEntity();
            }
            
            if (_targetEntity == Entity.Null) {
                //_targetEntity = GetRandomEntity();
                return;
            }
            var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
            // if (!entityManager.Exists(_targetEntity)) {
            //     _targetEntity = GetRandomEntity();
            //     return;
            // }
            var position = entityManager.GetComponentData<LocalToWorld>(_targetEntity).Position;
            transform.position = position;
        }

        private Entity GetRandomEntity() {
            var entities = World.DefaultGameObjectInjectionWorld.EntityManager.GetAllEntities();
            if (entities.Length == 0) return Entity.Null;
            return entities[Random.Range(0, entities.Length)];
        }
        
    }
}