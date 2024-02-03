using Unity.Entities;
using UnityEngine;

namespace PlayerCamera.Components {
    [DisallowMultipleComponent]
    [AddComponentMenu("Movable")]
    public class MovableAuthoring : MonoBehaviour {
        public float moveSpeed;
        public float rotateSpeed;
        private class MovableAuthoringBaker : Baker<MovableAuthoring> {
            public override void Bake(MovableAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new MovableComponentData {
                    MoveSpeed = authoring.moveSpeed,
                    RotateSpeed = authoring.rotateSpeed
                });
            }
        }
    }
}