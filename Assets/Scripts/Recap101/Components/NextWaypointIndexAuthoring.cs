using Unity.Entities;
using UnityEngine;

namespace Recap101.Components {
    public struct NextWaypointIndexComponent : IComponentData {
        public int Value;
    }

    public class NextWaypointIndexAuthoring : MonoBehaviour {
        [SerializeField] private int value;

        private class NextWaypointIndexBaker : Baker<NextWaypointIndexAuthoring> {
            public override void Bake(NextWaypointIndexAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new NextWaypointIndexComponent { Value = authoring.value });
            }
        }
    }
}