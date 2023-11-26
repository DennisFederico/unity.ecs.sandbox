using System;
using Crowds.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Crowds {
    public class SelectionMarker : MonoBehaviour {
        private Entity _selectedEntity;
        private EntityManager _entityManager;

        private void Start() {
             _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        }

        private void LateUpdate() {
            if (Input.GetKeyDown(KeyCode.Space)) {
                _selectedEntity = SelectRandomEntity(typeof(CrowdMemberTag));
            }

            if (_selectedEntity != Entity.Null) {
                
                if (!_entityManager.Exists(_selectedEntity)) {
                    _selectedEntity = Entity.Null;
                    return;
                }
                transform.position = _entityManager.GetComponentData<LocalTransform>(_selectedEntity).Position;
            }
        }

        private Entity SelectRandomEntity(ComponentType componentType) {
            var entityQuery = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(componentType);
            var entityArray = entityQuery.ToEntityArray(Allocator.Temp);
            return entityArray.Length == 0 ?  Entity.Null : entityArray[Random.Range(0, entityArray.Length)];
        }
    }
}