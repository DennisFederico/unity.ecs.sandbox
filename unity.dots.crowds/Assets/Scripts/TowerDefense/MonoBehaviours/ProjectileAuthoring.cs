using TowerDefense.Components;
using Unity.Entities;
using UnityEngine;

namespace TowerDefense.MonoBehaviours {
    public class ProjectileAuthoring : MonoBehaviour {
        
        [SerializeField] private float speed;
        
        private class ProjectileAuthoringBaker : Baker<ProjectileAuthoring> {
            public override void Bake(ProjectileAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new MoveSpeedComponent() {
                    Value = authoring.speed
                });
            }
        }
    }
}