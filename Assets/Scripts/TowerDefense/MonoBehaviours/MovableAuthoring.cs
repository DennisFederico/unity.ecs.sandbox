using TowerDefense.Components;
using Unity.Entities;
using UnityEngine;

namespace TowerDefense.MonoBehaviours {
    public class MovableAuthoring : MonoBehaviour {
        [SerializeField] private float speed = 3f;
        private class MovableAuthoringBaker : Baker<MovableAuthoring> {
            public override void Bake(MovableAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                if (authoring.speed > 0) {
                    AddComponent(entity, new MoveSpeedComponent {Value = authoring.speed});    
                }
            }
        }
    }
}