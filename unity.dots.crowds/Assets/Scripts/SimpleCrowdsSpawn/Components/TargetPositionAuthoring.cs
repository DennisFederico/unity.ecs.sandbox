using Unity.Entities;
using UnityEngine;

namespace SimpleCrowdsSpawn.Components {
    public class TargetPositionAuthoring : MonoBehaviour {
        [SerializeField] private Vector3 targetPosition = Vector3.zero;
        [SerializeField] private bool randomize;

        private class TargetPositionAuthoringBaker : Baker<TargetPositionAuthoring> {
            public override void Bake(TargetPositionAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.None);
                var targetPosition = authoring.randomize
                    ? new Vector3() {
                        x = Random.Range(-15f, 15f),
                        y = 0f,
                        z = Random.Range(-15f, 15f)
                    }
                    : authoring.targetPosition;
                AddComponent(entity, new TargetPosition { Value = targetPosition });
                // Debug.Log($"Baked component TargetPosition with value {targetPosition} to entity {entity}");
            }
        }
    }
}