using System.Collections.Generic;
using System.Linq;
using TowerDefense.Components;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace TowerDefense.MonoBehaviours {
    public class SpawnerAuthoring : MonoBehaviour {
        
        [SerializeField] private GameObject prefab;
        [SerializeField] private float spawnInterval = 5f;
        public List<Transform> SpawnPoints => GetComponentsInChildren<Transform>().Where(go => go.gameObject != gameObject).ToList();
        private class SpawnerAuthoringBaker : Baker<SpawnerAuthoring> {
            public override void Bake(SpawnerAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Renderable);
                
                // var waypoints = AddBuffer<WaypointsComponent>(entity);
                // foreach (var spawnPoint in authoring.SpawnPoints) {
                //     waypoints.Add(new WaypointsComponent {Value = spawnPoint.position});
                // }

                BlobAssetReference<BlobPath> blobPath;
                using (BlobBuilder blobBuilder = new BlobBuilder(Allocator.Temp)) {
                    ref BlobPath blobPathRef = ref blobBuilder.ConstructRoot<BlobPath>();
                    
                    BlobBuilderArray<float3> waypoints = blobBuilder.Allocate(ref blobPathRef.Waypoints, authoring.SpawnPoints.Count);
                    for (int i = 0; i < authoring.SpawnPoints.Count; i++) {
                        waypoints[i] = authoring.SpawnPoints[i].position;
                    }
                    blobPath = blobBuilder.CreateBlobAssetReference<BlobPath>(Allocator.Persistent);
                }
                AddComponent(entity, new BlobPathAsset {Path = blobPath});
                
                AddComponent(entity, new SpawnerDataComponent {
                    Prefab = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic), 
                    SpawnInterval = authoring.spawnInterval,
                    SpawnTimer = authoring.spawnInterval
                });
            }
        }
    }
}