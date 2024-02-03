using Unity.Entities;
using UnityEngine;

namespace Recap101.Components {
    public class MoveMeAuthoring : MonoBehaviour {
        [SerializeField] private float speed = 3f;
        private class MoveMeAuthoringBaker : Baker<MoveMeAuthoring> {
            public override void Bake(MoveMeAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                if (authoring.speed > 0) {
                    AddComponent(entity, new MoveSpeedComponent {Value = authoring.speed});    
                }
                AddComponent(entity, new TagComponent());
            }
        }
    }
}