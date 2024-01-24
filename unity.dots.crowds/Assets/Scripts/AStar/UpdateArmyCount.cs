using AStar.Components;
using Unity.Entities;
using UnityEngine;

namespace AStar {
    public class UpdateArmyCount : MonoBehaviour {
        [SerializeField] private TMPro.TextMeshProUGUI armyCountText;

        private EntityManager _entityManager;
        private float _maxTimer = 0.5f;
        private float _timer = 0f;
        
        private void Start() {
            _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        }
            
        private void Update() {
            _timer += Time.deltaTime;
            if (_timer < _maxTimer) return;
            _timer = 0f;
            var entityCount = _entityManager.CreateEntityQuery(typeof(PathFindingUserTag)).CalculateEntityCount();
            armyCountText.text = $"Army Count: {entityCount}";
        }
    }
}