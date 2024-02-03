using Unity.Entities;
using UnityEngine;

namespace SimpleCrowdsSpawn.Components {

    public struct RandomComponent : IComponentData {
        public Unity.Mathematics.Random Value;
    }

    public class RandomAuthoring : MonoBehaviour {
        private class RandomAuthoringBaker : Baker<RandomAuthoring> {
            public override void Bake(RandomAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new RandomComponent { Value = new Unity.Mathematics.Random((uint)Random.Range(1, uint.MaxValue)) });
            }
        }
    }
}