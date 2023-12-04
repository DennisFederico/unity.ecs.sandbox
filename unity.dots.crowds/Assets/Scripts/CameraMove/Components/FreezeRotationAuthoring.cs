using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

namespace CameraMove.Components {
    
    [DisallowMultipleComponent]
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