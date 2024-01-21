using Switching.Components;
using Unity.Entities;
using UnityEngine;

namespace Switching.MonoBehaviours {
    public class VisualRepresentationTagAuthoring : MonoBehaviour {
        [SerializeField] private Color color = Color.cyan;
        private class VisualRepresentationTagAuthoringBaker : Baker<VisualRepresentationTagAuthoring> {
            public override void Bake(VisualRepresentationTagAuthoring authoring) {
                Color _color = authoring.color;
                // if (GetParent() && GetParent().GetComponent<TeamMemberAuthoring>().IsPlaying) {
                //     var team = GetParent().GetComponent<TeamMemberAuthoring>().Team;
                //     _color = team == Team.Blue ? Color.blue : Color.red;
                // }
                
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<VisualRepresentationTag>(entity);
                //AddComponent(entity, new URPMaterialPropertyBaseColor() { Value = new float4(_color.r, _color.g, _color.b, _color.a) });
            }
        }
    }
}