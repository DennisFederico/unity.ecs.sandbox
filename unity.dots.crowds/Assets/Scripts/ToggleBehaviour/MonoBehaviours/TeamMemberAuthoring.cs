using ToggleBehaviour.Components;
using Unity.Entities;
using UnityEngine;

namespace ToggleBehaviour.MonoBehaviours {
    public class TeamMemberAuthoring : MonoBehaviour {
        
        [SerializeField] private string playerName;
        [SerializeField] private Team team;
        [SerializeField] private bool isPlaying;
        private class PlayerNameAuthoringBaker : Baker<TeamMemberAuthoring> {
            public override void Bake(TeamMemberAuthoring authoring) {
                //TODO user an Archetype instead
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new PlayerNameComponent(authoring.playerName));
                AddComponent(entity, new TeamMemberComponent() { Team = authoring.team });
                AddComponent<IsSelectedComponentTag>(entity);
                SetComponentEnabled<IsSelectedComponentTag>(entity, authoring.team == Team.Blue);
                AddComponent<IsPlayingComponentTag>(entity);
                SetComponentEnabled<IsPlayingComponentTag>(entity, authoring.isPlaying);
            }
        }
    }
}