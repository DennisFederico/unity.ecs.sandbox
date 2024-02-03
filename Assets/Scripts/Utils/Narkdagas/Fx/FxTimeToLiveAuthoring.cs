using Unity.Entities;
using UnityEngine;

namespace Utils.Narkdagas.Fx {
    public class FxTimeToLiveAuthoring : MonoBehaviour {
        [SerializeField] private float duration;

        private class TimeToLiveAuthoringBaker : Baker<FxTimeToLiveAuthoring> {
            public override void Bake(FxTimeToLiveAuthoring authoring) {
                float duration = authoring.duration;
                if (authoring.gameObject.TryGetComponent<ParticleSystem>(out var particleSystem)) {
                    duration = particleSystem.main.duration;
                }
                AddComponent(GetEntity(TransformUsageFlags.Renderable), new FxTimeToLiveComponent { Value = duration });
            }
        }
    }
}