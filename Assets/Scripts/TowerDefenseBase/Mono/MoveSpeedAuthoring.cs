using TowerDefenseBase.Components;
using Unity.Entities;
using UnityEngine;

namespace TowerDefenseBase.Mono {
    public class MoveSpeedAuthoring : MonoBehaviour {
        [SerializeField] private float speed = 3f;
        private class MovableAuthoringBaker : Baker<MoveSpeedAuthoring> {
            public override void Bake(MoveSpeedAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                if (authoring.speed > 0) {
                    AddComponent(entity, new MoveSpeedComponent {Value = authoring.speed});    
                }
            }
        }
    }
}