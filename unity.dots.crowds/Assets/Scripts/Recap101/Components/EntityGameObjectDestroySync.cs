using Unity.Entities;
using UnityEngine;

namespace Recap101.Components {
    public class EntityGameObjectDestroySync : MonoBehaviour {
        private Entity _entity;
        private EntityManager _entityManager;
        
        public void SetEntity(Entity entity, EntityManager entityManager) {
            _entity = entity;
            _entityManager = entityManager;
        }
        
        private void OnDestroy() {
            _entityManager.DestroyEntity(_entity);
        }
    }
}