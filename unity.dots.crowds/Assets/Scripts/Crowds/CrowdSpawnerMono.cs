using System.Collections.Generic;
using UnityEngine;

namespace Crowds {
    public class CrowdSpawnerMono : MonoBehaviour {
        [SerializeField] private GameObject _prefab;
        private List<GameObject> _crowdMembers = new List<GameObject>();
        private float timer = 0f;
        private void LateUpdate() {
            int maxCrowdSize = 0;
            if (_crowdMembers.Count < maxCrowdSize) {
                for (int i = 0; i < 100; i++) {
                    var crowdMember = Instantiate(_prefab, transform.position, Quaternion.identity);
                    crowdMember.AddComponent<CrowdMemberMono>();
                    _crowdMembers.Add(crowdMember);    
                }
            }
            timer += Time.deltaTime;
            if (timer > 1f) {
                Debug.Log($"Crowd size Mono: {_crowdMembers.Count}");
                timer = 0f;
            }
        }
    }
}