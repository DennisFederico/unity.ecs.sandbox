using Unity.Entities;

namespace Selection.Components {
    public struct SelectedVisualPrefabComponent : IComponentData {
        public Entity Value;
    }
}