using Unity.Entities;

namespace Crowds.Components {
    public struct CrowdSpawner : IComponentData {
        public Entity Prefab;
    }
}