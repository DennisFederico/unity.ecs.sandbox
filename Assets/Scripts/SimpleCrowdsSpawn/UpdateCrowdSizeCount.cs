using SimpleCrowdsSpawn.Components;
using Unity.Entities;
using UnityEngine;

namespace SimpleCrowdsSpawn {
    public class UpdateCrowdSizeCount : MonoBehaviour {
        [SerializeField] private TMPro.TextMeshProUGUI crowdSizeText;
        
        private EntityManager _entityManager;
        private const float MaxTimer = 0.5f;
        private float _timer;
        
        private void Start() {
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        }
        
        private void LateUpdate() {
            _timer += Time.deltaTime;
            if (_timer < MaxTimer) return;
            _timer = 0f;
            var crowdSize = _entityManager.CreateEntityQuery(typeof(CrowdMemberTag)).CalculateEntityCount();
            crowdSizeText.text = $"{crowdSize}";
        }
    }
}