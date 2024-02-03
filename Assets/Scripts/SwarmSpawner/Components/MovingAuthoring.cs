using Unity.Entities;
using UnityEngine;

namespace SwarmSpawner.Components {
    public class MovingAuthoring : MonoBehaviour {

        public float moveSpeed;
        public float rotateSpeed;
        
        private class MovingAuthoringBaker : Baker<MovingAuthoring> {
            public override void Bake(MovingAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new MovingComponentData {
                    MoveSpeed = authoring.moveSpeed,
                    RotateSpeed = authoring.rotateSpeed
                });
            }
        }
    }
}