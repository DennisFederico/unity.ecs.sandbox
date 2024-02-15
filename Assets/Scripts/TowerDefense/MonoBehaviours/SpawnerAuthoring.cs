using TowerDefense.Components;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace TowerDefense.MonoBehaviours {
    public class SpawnerAuthoring : MonoBehaviour {

        [SerializeField] private GameObject prefab;
        [SerializeField] private float spawnInterval = 5f;
        //TRIED WITH A REFERENCE TO AN SCRIPTABLE OBJECT BUT DIDN'T WORK PROPERLY 
        [SerializeField] private Transform parentWaypoints;

        private class SpawnerAuthoringBaker : Baker<SpawnerAuthoring> {
            public override void Bake(SpawnerAuthoring authoring) {
                DependsOn(authoring.parentWaypoints);
                var waypointsCount = authoring.parentWaypoints.childCount;
                var entity = GetEntity(TransformUsageFlags.None);
                
                BlobAssetReference<WaypointsArray> blobAssetReference;
                
                using (var blobBuilder = new BlobBuilder(Allocator.Temp)) {
                    ref var waypointsArrayRef = ref blobBuilder.ConstructRoot<WaypointsArray>();
                    var waypointsArray = blobBuilder.Allocate(ref waypointsArrayRef.Waypoints, waypointsCount);
                    for (int i = 0; i < waypointsCount; i++) {
                        waypointsArray[i] = authoring.parentWaypoints.GetChild(i).position;
                    }
                    blobAssetReference = blobBuilder.CreateBlobAssetReference<WaypointsArray>(Allocator.Persistent);
                }

                AddComponent(entity, new WaypointsAsset { Path = blobAssetReference });
                AddComponent(entity, new SpawnerDataComponent {
                    Prefab = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic),
                    SpawnInterval = authoring.spawnInterval,
                    SpawnTimer = authoring.spawnInterval
                });
            }
        }
    }
}