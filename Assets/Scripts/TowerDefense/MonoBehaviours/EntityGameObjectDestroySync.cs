using Unity.Entities;
using UnityEngine;

namespace TowerDefense.MonoBehaviours {
    public class EntityGameObjectDestroySync : MonoBehaviour {
        private Entity _entity;
        private EntityManager _entityManager;
        private World _world;
        
        private void OnEnable() {
            _world = World.DefaultGameObjectInjectionWorld;
        }
        
        public void SetEntity(Entity entity) {
            _entity = entity;
        }

        private void OnDestroy() {
            if (_world.IsCreated && _world.EntityManager.Exists(_entity)) {
                _world.EntityManager.DestroyEntity(_entity);
            }
        }
    }
}