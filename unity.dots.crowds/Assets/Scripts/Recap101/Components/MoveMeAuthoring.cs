using Unity.Entities;
using UnityEngine;

namespace Recap101.Components {
    public class MoveMeAuthoring : MonoBehaviour {
        [SerializeField] private float speed = 3f;
        private class MoveMeAuthoringBaker : Baker<MoveMeAuthoring> {
            public override void Bake(MoveMeAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new MoveSpeed {Value = authoring.speed});
            }
        }
    }
}