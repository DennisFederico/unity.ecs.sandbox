using Unity.Entities;
using UnityEngine;

namespace Crowds.Components {
    public class RandomAuthoring : MonoBehaviour {
        [SerializeField] private uint seed = 1;

        private class RandomAuthoringBaker : Baker<RandomAuthoring> {
            public override void Bake(RandomAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.None);
                //AddComponent(entity, new RandomComponent { Value = new Unity.Mathematics.Random((uint)Random.Range(0, int.MaxValue)) });
                AddComponent(entity, new RandomComponent { Value = new Unity.Mathematics.Random(authoring.seed) });
            }
        }
    }
}