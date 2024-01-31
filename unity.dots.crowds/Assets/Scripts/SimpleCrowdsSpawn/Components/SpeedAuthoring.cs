using Unity.Entities;
using UnityEngine;

namespace SimpleCrowdsSpawn.Components {
    
    public struct Speed : IComponentData {
        public float Value;
    }
    
    public class SpeedAuthoring : MonoBehaviour {
        [SerializeField] private float speed = 1f;

        private class SpeedAuthoringBaker : Baker<SpeedAuthoring> {
            public override void Bake(SpeedAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Speed { Value = authoring.speed });
            }
        }
    }
}