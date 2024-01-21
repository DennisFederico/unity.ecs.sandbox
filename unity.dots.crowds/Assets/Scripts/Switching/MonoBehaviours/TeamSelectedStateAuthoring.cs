using Switching.Components;
using Unity.Entities;
using UnityEngine;

namespace Switching.MonoBehaviours {
    public class TeamSelectedStateAuthoring : MonoBehaviour {
        [SerializeField] private Team team;
        private class TeamSelectedStateAuthoringBaker : Baker<TeamSelectedStateAuthoring> {
            public override void Bake(TeamSelectedStateAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new TeamSelectedStateComponent() { Team = authoring.team });
            }
        }
    }
}