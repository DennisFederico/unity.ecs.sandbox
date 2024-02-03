using System;
using SimpleCrowdsSpawn.Systems;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

namespace SimpleCrowdsSpawn {
    
    /// <summary>
    /// There's no Animator counterpart in ECS.
    /// It keeps track of a selected entity and moves the selection marker to its position on every frame lateUpdate.
    /// </summary>
    public class SelectionMarkerManager : MonoBehaviour {
        
        [SerializeField] private GameObject selectionMarkerPrefab;
        private GameObject _selectionMarkerInstance;
        private World _world;
        private PlaceSpawnerSystem _placeSpawnerSystem;
        private Entity _selectedEntity;
        
        
        private void OnEnable() {
            _selectionMarkerInstance = Instantiate(selectionMarkerPrefab);
            _selectionMarkerInstance.SetActive(false);
            _world = World.DefaultGameObjectInjectionWorld;
            if (_world.IsCreated) {
                _placeSpawnerSystem = _world.GetExistingSystemManaged<PlaceSpawnerSystem>();
                _placeSpawnerSystem.EntitySelected += OnEntitySelected;
            }
        }

        private void OnDisable() {
            if (_world.IsCreated && _placeSpawnerSystem != null) _placeSpawnerSystem.EntitySelected -= OnEntitySelected;
            if (_selectionMarkerInstance) Destroy(_selectionMarkerInstance);
        }

        private void OnEntitySelected(Entity entity) {
            _selectedEntity = entity;
        }

        private void LateUpdate() {
            if (_selectedEntity != Entity.Null && _world.IsCreated && _world.EntityManager.Exists(_selectedEntity)) {
                if (!_selectionMarkerInstance.activeSelf) _selectionMarkerInstance.SetActive(true);
                _selectionMarkerInstance.transform.position = _world.EntityManager.GetComponentData<LocalTransform>(_selectedEntity).Position;
            }
        }

        // private void OnDrawGizmos() {
        //     if (_selectedEntity != Entity.Null && _world.IsCreated && _world.EntityManager.Exists(_selectedEntity)) {
        //         var position = _world.EntityManager.GetComponentData<LocalTransform>(_selectedEntity).Position;
        //         Gizmos.color = Color.red;
        //         Gizmos.DrawSphere(position, 0.5f);
        //     }
        // }
    }
}