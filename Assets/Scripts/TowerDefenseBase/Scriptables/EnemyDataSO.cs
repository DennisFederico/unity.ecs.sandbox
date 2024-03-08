using UnityEngine;

namespace TowerDefenseBase.Scriptables {
    [CreateAssetMenu(menuName = "TowerDefense/EnemyDataSO", order = 0)]
    public class EnemyDataSO : ScriptableObject {
        [Range(1f, 5f)]
        public float speed = 3f;
        [Range(1f, 200f)]
        public float health = 100f;
        public GameObject visualPrefab;
    }
}