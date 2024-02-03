using System;
using Switching.Components;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Switching.Systems {

    /// <summary>
    /// The idea of this System is to be able to dispatch events from the ECS world to the Unity world.
    /// It fires events on state changes.
    /// </summary>
    [UpdateInGroup(typeof(LateSimulationSystemGroup))]
    public partial class StateEventDispatcherSystem : SystemBase {

        public struct SimulationState {
            public Team SelectedTeam;
            public int SelectedCounter;
        }

        public event Action<SimulationState> SimulationStateChangedEvent;

        private SimulationState _simulationState;
        private EntityQuery _selectedPlayerQuery;

        protected override void OnCreate() {
            
            _simulationState = new SimulationState {
                SelectedTeam = Team.None,
                SelectedCounter = 0
            };

            _selectedPlayerQuery = SystemAPI.QueryBuilder()
                .WithAll<PlayerNameComponent>()
                .WithAll<IsSelectedComponentTag>()
                .WithAll<IsPlayingComponentTag>()
                .WithAll<TeamMemberComponent>()
                .Build();
        }
        
        protected override void OnUpdate() {
            //NOTE There are some "hidden" players (benched) with disabled IsPlayingComponentTag, EntityQuery still matches them
            if (_selectedPlayerQuery.IsEmpty) return;
            var selectedCounter = _selectedPlayerQuery.CalculateEntityCount();
            var selectedTeam = _selectedPlayerQuery.ToComponentDataArray<TeamMemberComponent>(Allocator.Temp)[0].Team;
            if (selectedCounter != _simulationState.SelectedCounter || selectedTeam != _simulationState.SelectedTeam) {
                _simulationState = new SimulationState() {
                    SelectedCounter = selectedCounter,
                    SelectedTeam = selectedTeam
                };
                SimulationStateChangedEvent?.Invoke(_simulationState);
            }
        }
    }
}