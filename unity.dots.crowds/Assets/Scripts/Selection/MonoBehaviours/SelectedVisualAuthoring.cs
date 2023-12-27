using Selection.Components;
using Unity.Entities;
using UnityEngine;

namespace Selection.MonoBehaviours {
    public class SelectedVisualAuthoring : MonoBehaviour {
        [SerializeField] private GameObject selectedVisualPrefab;
        
        private class SelectionUIAuthoringBaker : Baker<SelectedVisualAuthoring> {
            public override void Bake(SelectedVisualAuthoring authoring) {
                var visualPrefab = GetEntity(authoring.selectedVisualPrefab, TransformUsageFlags.Dynamic);
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new SelectedVisualPrefabComponent() { Value = visualPrefab });
            }
        }
    }
}