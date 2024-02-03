using System.Collections.Generic;
using System.Linq;
using Unity.Entities;
using UnityEngine;

namespace Recap101.Components {
    public class SpawnerAuthoring : MonoBehaviour {
        
        [SerializeField] private GameObject prefab;
        [SerializeField] private float spawnInterval = 5f;
        public List<Transform> SpawnPoints => GetComponentsInChildren<Transform>().Where(go => go.gameObject != gameObject).ToList();
        private class SpawerAuthoringBaker : Baker<SpawnerAuthoring> {
            public override void Bake(SpawnerAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.None);
                var waypoints = AddBuffer<WaypointsComponent>(entity);
                foreach (var spawnPoint in authoring.SpawnPoints) {
                    waypoints.Add(new WaypointsComponent {Value = spawnPoint.position});
                }
                
                AddComponent(entity, new SpawnerComponent {
                    Prefab = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic), 
                    SpawnInterval = authoring.spawnInterval,
                    SpawnTimer = authoring.spawnInterval
                });
            }
        }
    }
}