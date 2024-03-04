using Unity.Mathematics;
using UnityEngine;

namespace TowerDefenseBase.Helpers {

    public class WaypointsToScriptableObject : MonoBehaviour {
        [Tooltip("Scriptable Object to store the waypoints")]
        [SerializeField] private WaypointsScriptableObject waypointsScriptableObject;
        [Tooltip("Transform to extract the position for the waypoints")]
        [SerializeField] private GameObject[] waypointsReferences;
        [Tooltip("The extracted positions")]
        [SerializeField] private Vector3[] positions;
        
        [ContextMenu("Store Positions as Waypoints")]
        public void StoreWaypoints() {
            var size = waypointsReferences.Length;
            if (size <= 0) return;
            waypointsScriptableObject.waypoints = new float3[size];
            positions = new Vector3[size];
            for (var i = 0; i < size; i++) {
                waypointsScriptableObject.waypoints[i] = waypointsReferences[i].transform.position;
                positions[i] = waypointsReferences[i].transform.position;
            }
        }
    }
}