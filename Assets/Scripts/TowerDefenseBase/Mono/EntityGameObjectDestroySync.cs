using Unity.Entities;
using UnityEngine;

namespace TowerDefenseBase.Mono {
    
    /// <summary>
    /// This class syncs the destruction of a GameObject with the destruction of its entity counterpart.
    /// </summary>
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