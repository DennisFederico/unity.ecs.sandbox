using Unity.Entities;
using UnityEngine;

namespace Crowds.Components {
    public class TargetPositionAuthoring : MonoBehaviour {
        [SerializeField] private Vector3 targetPosition = Vector3.zero;
        private class TargetPositionAuthoringBaker : Baker<TargetPositionAuthoring> {
            public override void Bake(TargetPositionAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new TargetPosition { Value = authoring.targetPosition });
            }
        }
    }
}