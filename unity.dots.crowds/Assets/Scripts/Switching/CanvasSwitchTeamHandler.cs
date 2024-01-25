using System;
using Switching.Components;
using Switching.Systems;
using TMPro;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Switching {
    public class CanvasSwitchTeamHandler : MonoBehaviour {

        [SerializeField] private GameObject blueTeamImage;
        [SerializeField] private GameObject redTeamImage;
        [SerializeField] private TextMeshProUGUI counterText;
        
        private World _world;
        private StateEventDispatcherSystem _stateEventDispatcherSystem;
        private DebugLogSystem _debugLogSystem;
        private bool _firstUpdate;

        private void OnEnable() {
            _world = World.DefaultGameObjectInjectionWorld;
            if (_world.IsCreated) {
                _stateEventDispatcherSystem = _world.GetOrCreateSystemManaged<StateEventDispatcherSystem>();
                _stateEventDispatcherSystem.SimulationStateChangedEvent += OnSimulationStateChangedEvent;
                _debugLogSystem = _world.GetOrCreateSystemManaged<DebugLogSystem>();
                _debugLogSystem.DebugLogEvent += OnDebugLogEvent;
            }
        }

        private void OnSimulationStateChangedEvent(StateEventDispatcherSystem.SimulationState state) {
            //HACK Skip the first update, because the state might not yet initialized
            if (!_firstUpdate) {
                _firstUpdate = true;
                return;
            }
            switch (state.SelectedTeam) {
                case Team.Blue:
                    blueTeamImage.SetActive(true);
                    redTeamImage.SetActive(false);
                    break;
                case Team.Red:
                    blueTeamImage.SetActive(false);
                    redTeamImage.SetActive(true);
                    break;
                case Team.None:
                    blueTeamImage.SetActive(false);
                    redTeamImage.SetActive(false);
                    break;
            }
            counterText.text = state.SelectedCounter.ToString();
        }

        private static void OnDebugLogEvent(FixedString128Bytes msg) {
            Debug.Log($"Event Received: {msg}");
        }

        private void OnDisable() {
            if (_world is { IsCreated: true }) {
                if (_debugLogSystem != null) {
                    _debugLogSystem.DebugLogEvent -= OnDebugLogEvent;
                }
                if (_stateEventDispatcherSystem != null) {
                    _stateEventDispatcherSystem.SimulationStateChangedEvent -= OnSimulationStateChangedEvent;
                }
            }
        }
    }
}