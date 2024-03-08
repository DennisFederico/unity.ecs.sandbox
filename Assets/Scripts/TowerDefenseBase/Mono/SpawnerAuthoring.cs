using TowerDefenseBase.Components;
using TowerDefenseBase.Scriptables;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Hash128 = UnityEngine.Hash128;

namespace TowerDefenseBase.Mono {
    public class SpawnerAuthoring : MonoBehaviour {

        [SerializeField] private GameObject prefab;

        [SerializeField] private float spawnInterval = 5f;

        [SerializeField] private WaypointsSO waypointsSO;
        //[SerializeField] DataContainer dataContainer;

        private class SpawnerAuthoringBaker : Baker<SpawnerAuthoring> {
            public override void Bake(SpawnerAuthoring authoring) {
                Debug.Log($"Running the baker for {authoring.name}...");
                // DependsOn(authoring.dataContainer);
                // DependsOn(authoring.dataContainer.WaypointsSO);
                DependsOn(authoring.waypointsSO);

                // if (authoring.dataContainer == null || authoring.dataContainer.WaypointsSO == null || authoring.dataContainer.WaypointsSO.Waypoints == null || authoring.dataContainer.WaypointsSO.Waypoints.Length == 0) {
                if (authoring.waypointsSO == null || authoring.waypointsSO.Waypoints == null || authoring.waypointsSO.Waypoints.Length == 0) {
                    Debug.Log($"Scriptable Object not loaded / found! Skipping...");
                    return;
                }
                
                //This is a copy of the array, so it's not the same reference
                var soName = authoring.waypointsSO.name;
                var waypoints = authoring.waypointsSO.Waypoints;

                //Create the blobAssetReference for the waypoints
                var assetHash = new Hash128();
                HashUtilities.ComputeHash128(System.Text.Encoding.UTF8.GetBytes(soName), ref assetHash);
                for (int i = 0; i < waypoints.Length; i++) {
                    var waypointHash = new Hash128();
                    HashUtilities.ComputeHash128(ref waypoints[i], ref waypointHash);
                    HashUtilities.AppendHash(ref waypointHash, ref assetHash);
                }

                if (!TryGetBlobAssetReference(assetHash, out BlobAssetReference<Waypoints> blobAssetReference)) {
                    Debug.Log($"Blob Asset not found for {soName}! Creating new one...");

                    using (var blobBuilder = new BlobBuilder(Allocator.Temp)) {
                        ref var waypointsRef = ref blobBuilder.ConstructRoot<Waypoints>();
                        var waypointsArray = blobBuilder.Allocate(ref waypointsRef.Points, waypoints.Length);
                        for (var i = 0; i < waypoints.Length; i++) {
                            // Debug.Log($"Processing waypoint {i}... {waypoints[i]}");
                            waypointsArray[i] = waypoints[i];
                        }

                        blobAssetReference = blobBuilder.CreateBlobAssetReference<Waypoints>(Allocator.Persistent);
                    }

                    //Register the blob asset to the baker for proper updating on change
                    AddBlobAssetWithCustomHash(ref blobAssetReference, assetHash);
                    //AddBlobAsset(ref blobAssetReference, out _);
                } else {
                    Debug.Log($"Blob Asset found for {soName}!");
                }

                var entity = GetEntity(TransformUsageFlags.None);

                AddComponent(entity, new SpawnerDataComponent {
                    Prefab = GetEntity(authoring.prefab, TransformUsageFlags.Dynamic),
                    SpawnInterval = authoring.spawnInterval,
                    SpawnTimer = authoring.spawnInterval
                });

                AddComponent(entity, new WaypointsAsset { Waypoints = blobAssetReference });
            }
        }

        private void OnDrawGizmosSelected() {
            // if (dataContainer == null || dataContainer.WaypointsSO == null || dataContainer.WaypointsSO.Waypoints == null || dataContainer.WaypointsSO.Waypoints.Length == 0) return;
            if (waypointsSO == null || waypointsSO.Waypoints == null || waypointsSO.Waypoints.Length == 0) return;

            // var waypoints = dataContainer.WaypointsSO.Waypoints;
            var waypoints = waypointsSO.Waypoints;
            for (var i = 0; i < waypoints.Length - 1; i++) {
                var start = waypoints[i];
                var end = waypoints[i + 1];
                Gizmos.color = Color.red;
                Gizmos.DrawWireCube(start, Vector3.one * 0.3f);
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(start, end);
            }
        }
    }
}