using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace PlayerCamera.Components {
    
    [DisallowMultipleComponent]
    [AddComponentMenu("FreezeRotation")]
    [RequireComponent(typeof(Rigidbody))]
    public class FreezeRotationAuthoring : MonoBehaviour {
        public bool3 flags;
        private class FreezeRotationAuthoringBaker : Baker<FreezeRotationAuthoring> {
            public override void Bake(FreezeRotationAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new FreezeRotationComponentData {Flags = authoring.flags});
            }
        }
    }
}