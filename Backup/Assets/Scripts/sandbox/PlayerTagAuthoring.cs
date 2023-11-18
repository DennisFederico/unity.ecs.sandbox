using Unity.Entities;
using UnityEngine;

namespace sandbox {
    
    public struct PlayerTag : IComponentData {
        
    }

    public class PlayerTagAuthoring : MonoBehaviour {
        public class PlayerTagBaker : Baker<PlayerTagAuthoring> {
            public override void Bake(PlayerTagAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new PlayerTag());
            }
        }
    }
}