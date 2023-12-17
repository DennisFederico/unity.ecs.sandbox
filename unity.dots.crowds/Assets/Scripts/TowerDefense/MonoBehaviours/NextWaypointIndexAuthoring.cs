using TowerDefense.Components;
using Unity.Entities;
using UnityEngine;

namespace TowerDefense.MonoBehaviours {
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