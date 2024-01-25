using Switching.Components;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;

namespace Switching.MonoBehaviours {
    public class VisualRepresentationTagAuthoring : MonoBehaviour {
        [SerializeField] private Color color = Color.cyan;
        private class VisualRepresentationTagAuthoringBaker : Baker<VisualRepresentationTagAuthoring> {
            public override void Bake(VisualRepresentationTagAuthoring authoring) {
                Color color = authoring.color;
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<VisualRepresentationTag>(entity);
                AddComponent(entity, new URPMaterialPropertyBaseColor() { Value = new float4(color.r, color.g, color.b, color.a) });
            }
        }
    }
}