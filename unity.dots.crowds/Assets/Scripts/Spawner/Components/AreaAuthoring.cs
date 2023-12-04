using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Spawner.Components {
    public class AreaAuthoring : MonoBehaviour {
        public Vector3 targetArea;

        private void OnDrawGizmosSelected() {
            Gizmos.color = Color.yellow;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, targetArea);
        }
        
        private void OnDrawGizmos() {
            Gizmos.color = Color.green;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(Vector3.zero, targetArea);
        }

        private class AreaAuthoringBaker : Baker<AreaAuthoring> {
            public override void Bake(AreaAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                var scale = authoring.transform.localScale;
                var area = new float3(
                    scale.x * authoring.targetArea.x,
                    scale.y * authoring.targetArea.y,
                    scale.z * authoring.targetArea.z
                );
                AddComponent(entity, new AreaComponentData { area = area });
            }
        }
    }
}