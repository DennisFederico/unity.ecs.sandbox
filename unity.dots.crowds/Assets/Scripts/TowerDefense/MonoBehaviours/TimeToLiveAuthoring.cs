using TowerDefense.Components;
using Unity.Entities;
using UnityEngine;

namespace TowerDefense.MonoBehaviours {
    public class TimeToLiveAuthoring : MonoBehaviour {
        [SerializeField] private float duration;

        private class TimeToLiveAuthoringBaker : Baker<TimeToLiveAuthoring> {
            public override void Bake(TimeToLiveAuthoring authoring) {
                float duration = authoring.duration;
                if (authoring.gameObject.TryGetComponent<ParticleSystem>(out var particleSystem)) {
                    duration = particleSystem.main.duration;
                }
                AddComponent(GetEntity(TransformUsageFlags.Renderable), new TimeToLiveComponent() { Value = duration });
            }
        }
    }
}