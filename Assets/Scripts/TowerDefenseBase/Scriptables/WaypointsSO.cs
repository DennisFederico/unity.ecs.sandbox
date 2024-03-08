using System;
using Unity.Mathematics;
using UnityEngine;

namespace TowerDefenseBase.Scriptables {
    
    [CreateAssetMenu(menuName = "TowerDefense/WaypointsSO", order = 0)]
    [Serializable]
    public class WaypointsSO : ScriptableObject {
        public float3[] Waypoints;
    }
}