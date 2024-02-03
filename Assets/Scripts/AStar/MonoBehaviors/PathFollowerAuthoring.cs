using AStar.Components;
using Unity.Entities;
using UnityEngine;

namespace AStar.MonoBehaviors {
    public class PathFollowerAuthoring : MonoBehaviour {
        
        [SerializeField] private float speed = 1f;
        
        private class PathFollowerAuthoringBaker : Baker<PathFollowerAuthoring> {
            public override void Bake(PathFollowerAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<PathFindingUserTag>(entity);
                AddComponent<PathFindingRequest>(entity);
                SetComponentEnabled<PathFindingRequest>(entity, false);
                AddComponent(entity, new MoveSpeed { Value = authoring.speed });
                AddBuffer<PathPositionElement>(entity);
                AddComponent(entity, new PathFollowIndex { Value = -1 });
                SetComponentEnabled<PathFollowIndex>(entity, false);
            }
        }
    }
}