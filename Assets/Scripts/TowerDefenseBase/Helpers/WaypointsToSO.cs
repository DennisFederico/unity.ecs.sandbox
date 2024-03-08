using TowerDefenseBase.Scriptables;
using Unity.Mathematics;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;

namespace TowerDefenseBase.Helpers {

    public class WaypointsToSO : MonoBehaviour {
        [Tooltip("Scriptable Object to store the waypoints")]
        [SerializeField] private WaypointsSO waypointsSO;
        [Tooltip("Transform to extract the position for the waypoints")]
        [SerializeField] private GameObject[] waypointsReferences;
        [Tooltip("The extracted positions")]
        [SerializeField] private Vector3[] positions;
        
        [ContextMenu("Store Positions as Waypoints")]
        public void StoreWaypoints() {
            var size = waypointsReferences.Length;
            if (size <= 0) return;
            var waypoints = new float3[size];
            positions = new Vector3[size];
            for (var i = 0; i < size; i++) {
                waypoints[i] = waypointsReferences[i].transform.position;
                positions[i] = waypointsReferences[i].transform.position;
            }
            waypointsSO.Waypoints = waypoints;
#if UNITY_EDITOR
            EditorUtility.SetDirty(waypointsSO);
#endif
            
        }
    }
}