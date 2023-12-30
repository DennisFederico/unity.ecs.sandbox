using ToggleBehaviour.Components;
using Unity.Entities;
using UnityEngine;

namespace ToggleBehaviour.MonoBehaviours {
    public class TeamMemberAuthoring : MonoBehaviour {
        
        [SerializeField] private string playerName;
        [SerializeField] private Team team;
        private class PlayerNameAuthoringBaker : Baker<TeamMemberAuthoring> {
            public override void Bake(TeamMemberAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new PlayerNameComponent(authoring.playerName));
                AddComponent(entity, new TeamMemberComponent() { Team = authoring.team });
                AddComponent<IsSelectedComponent>(entity);
                SetComponentEnabled<IsSelectedComponent>(entity, authoring.team == Team.Blue);
            }
        }
    }
}