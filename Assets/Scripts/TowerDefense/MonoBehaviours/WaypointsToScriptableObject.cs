using Unity.Mathematics;
using UnityEngine;

namespace TowerDefense.MonoBehaviours {

    public class WaypointsToScriptableObject : MonoBehaviour {
        [SerializeField] WaypointsScriptableObject waypointsScriptableObject;
        [SerializeField] GameObject[] waypointsReferences;
        [SerializeField] Vector3[] positions;


        [ContextMenu("Store Positions as Waypoints")]
        void StoreWaypoints() {
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