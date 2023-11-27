using Unity.Entities;
using UnityEngine;

namespace Crowds.Components {
    public class RandomSeederAuthoring : MonoBehaviour {
        [SerializeField] private bool randomizeSeed;
        [SerializeField] private uint seed;
        private class RandomSeederAuthoringBaker : Baker<RandomSeederAuthoring> {
            public override void Bake(RandomSeederAuthoring authoring) {
                var mySeed = authoring.randomizeSeed ? (uint)Random.Range(1, uint.MaxValue) : authoring.seed;
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new RandomSeeder() { NextSeed = new Unity.Mathematics.Random(mySeed) });
            }
        }
    }
}