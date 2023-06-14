using Unity.Entities;
using UnityEngine;

namespace sandbox {
    public class SpeedAuthoring : MonoBehaviour {
        public float value;
    }

    public class SpeedBaker : Baker<SpeedAuthoring> {
        public override void Bake(SpeedAuthoring authoring) {
            AddComponent(GetEntity(authoring, TransformUsageFlags.Dynamic), new Speed {
                Value = authoring.value
            });
        }
    }
}