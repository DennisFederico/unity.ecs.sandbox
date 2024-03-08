using UnityEngine;

namespace TowerDefenseBase.Scriptables {
    [CreateAssetMenu(menuName = "TowerDefense/TurretDataSO", order = 0)]
    public class TurretDataSO : ScriptableObject {
        public Transform turretPrefab;
        public Transform ghostPrefab;
        public Transform bulletPrefab;
        [Range(2, 10)]
        public float fovRange = 2f;
        [Range(30f, 360f)]
        public float fovAngle = 30f;
        public float shootsPerSecond = 1f;
    }
}