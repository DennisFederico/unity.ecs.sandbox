using TowerDefenseBase.Components;
using TowerDefenseBase.Helpers;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace TowerDefenseBase.Mono {
    public class SpawnerAuthoring : MonoBehaviour {

        [SerializeField] private GameObject prefab;
        [SerializeField] private float spawnInterval = 5f;
        [SerializeField] private WaypointsScriptableObject waypointsScriptableObject;
        //TRIED WITH A REFERENCE TO AN SCRIPTABLE OBJECT BUT DIDN'T WORK PROPERLY 
        //[SerializeField] private Transform parentWaypoints;

        private class SpawnerAuthoringBaker : Baker<SpawnerAuthoring> {
            public override void Bake(SpawnerAuthoring authoring) {
                DependsOn(authoring.waypointsScriptableObject);
                
                var waypointsCount = authoring.waypointsScriptableObject.waypoints.Length;
                var entity = GetEntity(TransformUsageFlags.None);

                AddComponent(entity, new SpawnerDataComponent {
                    Prefab = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic),
                    SpawnInterval = authoring.spawnInterval,
                    SpawnTimer = authoring.spawnInterval
                });
                
                BlobAssetReference<Waypoints> blobAssetReference;
                
                using (var blobBuilder = new BlobBuilder(Allocator.Temp)) {
                    ref var waypointsRef = ref blobBuilder.ConstructRoot<Waypoints>();
                    var waypointsArray = blobBuilder.Allocate(ref waypointsRef.Points, waypointsCount);
                    for (var i = 0; i < waypointsCount; i++) {
                        waypointsArray[i] = authoring.waypointsScriptableObject.waypoints[i];
                    }
                    blobAssetReference = blobBuilder.CreateBlobAssetReference<Waypoints>(Allocator.Persistent);
                }
                AddComponent(entity, new WaypointsAsset { Waypoints = blobAssetReference });
                
                //Register the blob asset to the baker for proper updating on change
                AddBlobAsset(ref blobAssetReference, out _);
            }
        }

        private void OnDrawGizmosSelected() {
            if (waypointsScriptableObject == null || waypointsScriptableObject.waypoints == null || waypointsScriptableObject.waypoints.Length == 0) return;
            for (var i = 0; i < waypointsScriptableObject.waypoints.Length - 1; i++) {
                var start = waypointsScriptableObject.waypoints[i];
                var end = waypointsScriptableObject.waypoints[i + 1];
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(start, Vector3.one * 0.3f);
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(start, end);
            }
        }
    }
}