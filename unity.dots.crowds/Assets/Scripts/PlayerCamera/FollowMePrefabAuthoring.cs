using Unity.Entities;
using UnityEngine;

namespace PlayerCamera {
    
    //Note we use a class and add it as ComponentObject to the entity
    //Because Transform is a reference type, we can't use it directly as a component
    public class SpawnFollowerComponentData : IComponentData {
        public Transform Instance;
        public bool Spawned;
        public bool Position;
        public bool Rotation;
    }
    
    public class FollowMePrefabAuthoring : MonoBehaviour {

        public Transform prefab;
        public bool position;
        public bool rotation;
        
        private class FollowMeCameraAuthoringBaker : Baker<FollowMePrefabAuthoring> {
            public override void Bake(FollowMePrefabAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponentObject(entity, new SpawnFollowerComponentData {
                    Instance = authoring.prefab,
                    Position = authoring.position,
                    Rotation = authoring.rotation
                });
            }
        }
    }
}