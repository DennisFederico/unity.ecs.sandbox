using SwarmSpawner.Systems;
using TMPro;
using Unity.Entities;
using UnityEngine;

namespace SwarmSpawner {
    
    public class BallCounter : MonoBehaviour {
        [SerializeField] private TextMeshProUGUI counterText;
        private BallCounterSystem _ballCounterSystem;
        private World _world;

        private void OnEnable() {
            _world = World.DefaultGameObjectInjectionWorld;
            if (_world.IsCreated) {
                _ballCounterSystem = _world.GetOrCreateSystemManaged<BallCounterSystem>();
                _ballCounterSystem.OnBallCountChanged += OnBallCountChanged;
            }
        }

        private void OnBallCountChanged(int count) {
            counterText.text = $"Bees: {count}";
        }
    }
}