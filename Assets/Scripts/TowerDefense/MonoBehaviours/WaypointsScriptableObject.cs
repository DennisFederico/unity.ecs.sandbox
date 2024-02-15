using Unity.Mathematics;
using UnityEngine;

namespace TowerDefense.MonoBehaviours {
    [CreateAssetMenu(menuName = "WaypointsScriptableObject", order = 0)]
    public class WaypointsScriptableObject : ScriptableObject {
        public float3[] waypoints;
    }
}