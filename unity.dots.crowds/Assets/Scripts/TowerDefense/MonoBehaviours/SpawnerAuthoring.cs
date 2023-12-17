using System.Collections.Generic;
using System.Linq;
using TowerDefense.Components;
using Unity.Entities;
using UnityEngine;

namespace TowerDefense.MonoBehaviours {
    public class SpawnerAuthoring : MonoBehaviour {
        
        [SerializeField] private GameObject prefab;
        [SerializeField] private float spawnInterval = 5f;
        public List<Transform> SpawnPoints => GetComponentsInChildren<Transform>().Where(go => go.gameObject != gameObject).ToList();
        private class SpawnerAuthoringBaker : Baker<SpawnerAuthoring> {
            public override void Bake(SpawnerAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Renderable);
                var waypoints = AddBuffer<WaypointsComponent>(entity);
                foreach (var spawnPoint in authoring.SpawnPoints) {
                    waypoints.Add(new WaypointsComponent {Value = spawnPoint.position});
                }
                
                AddComponent(entity, new SpawnerDataComponent {
                    Prefab = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic), 
                    SpawnInterval = authoring.spawnInterval,
                    SpawnTimer = authoring.spawnInterval
                });
            }
        }
    }
}