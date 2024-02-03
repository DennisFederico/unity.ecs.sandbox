using System.Collections.Generic;
using UnityEngine;

namespace SimpleCrowdsSpawn {
    public class CrowdSpawnerMono : MonoBehaviour {
        [SerializeField] private GameObject prefab;
        private readonly List<GameObject> _crowdMembers = new List<GameObject>();
        private float _timer;

        private void LateUpdate() {
            int maxCrowdSize = 0;
            if (_crowdMembers.Count >= maxCrowdSize) return;

            for (int i = 0; i < 100; i++) {
                var crowdMember = Instantiate(prefab, transform.position, Quaternion.identity);
                crowdMember.AddComponent<CrowdMemberMono>();
                _crowdMembers.Add(crowdMember);
            }

            _timer += Time.deltaTime;
            if (_timer > 1f) {
                Debug.Log($"Crowd size Mono: {_crowdMembers.Count}");
                _timer = 0f;
            }
        }
    }
}