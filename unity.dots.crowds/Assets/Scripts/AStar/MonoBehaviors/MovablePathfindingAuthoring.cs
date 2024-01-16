using AStar.Components;
using Unity.Entities;
using UnityEngine;

namespace AStar.MonoBehaviors {

    public class MovablePathfindingAuthoring : MonoBehaviour {

        [SerializeField] private float speed = 1f;
        [SerializeField] private GameObject startPoint;
        [SerializeField] private GameObject endPoint;

        private class MovableUnitAuthoringBaker : Baker<MovablePathfindingAuthoring> {
            public override void Bake(MovablePathfindingAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<PathFindingUserTag>(entity);
                AddComponent<PathFindingRequest>(entity);
                SetComponent(entity, new PathFindingRequest {
                    StartPosition = authoring.startPoint.transform.position,
                    EndPosition = authoring.endPoint.transform.position
                });
                AddComponent(entity, new MoveSpeed() { Value = authoring.speed });
                AddBuffer<PathPositionElement>(entity);
                AddComponent(entity, new PathFollowIndex { Value = -1 });
            }
        }
    }
}