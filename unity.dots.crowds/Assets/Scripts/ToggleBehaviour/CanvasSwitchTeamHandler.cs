using System;
using TMPro;
using ToggleBehaviour.Systems;
using Unity.Entities;
using UnityEngine;

namespace ToggleBehaviour {
    public class CanvasSwitchTeamHandler : MonoBehaviour {

        [SerializeField] private GameObject blueTeamImage;
        [SerializeField] private GameObject redTeamImage;
        [SerializeField] private TextMeshProUGUI scoreText;
        
        private World _world;
        private DebugLogSystem _debugLogSystem;
        private bool _isBlueTeamActive;
        private float _timer;
        private float _waitTime = 2f;
        private int _counter = 0;

        private void OnEnable() {
            _world = World.DefaultGameObjectInjectionWorld;
            
            if (_world.IsCreated) {
                _debugLogSystem = _world.GetOrCreateSystemManaged<DebugLogSystem>();
                _debugLogSystem.DebugLogEvent += OnDebugLogEvent;
            }
        }

        private void OnDebugLogEvent(object sender, EventArgs e) {
            Debug.Log($"Event Received: {sender}");
            _counter++;
            _timer = _waitTime;
            _isBlueTeamActive = !_isBlueTeamActive;
            if (_isBlueTeamActive) {    
                blueTeamImage.SetActive(true);
                redTeamImage.SetActive(false);
            } else {
                blueTeamImage.SetActive(false);
                redTeamImage.SetActive(true);
            }
            scoreText.text = $"{_counter}";
        }

        private void OnDisable() {
            if (_world.IsCreated) {
                _debugLogSystem.DebugLogEvent -= OnDebugLogEvent;
            }
        }
        
        private void Update() {
            _timer -= Time.deltaTime;
            if (_timer < 0f && _counter != 0) {
                _counter = 0;
            }
        }
    }
}