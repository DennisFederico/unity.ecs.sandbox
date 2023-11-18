using Unity.Entities;
using UnityEngine;
using Random = Unity.Mathematics.Random;

namespace sandbox {
    
    public class RandomAuthoring : MonoBehaviour {
        public int seed;

        public class RandomAuthoringBaker : Baker<RandomAuthoring> {
            public override void Bake(RandomAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new RandomComponent { Seed = authoring.seed, Random = new Random((uint) authoring.seed)});
            }
        }
    }

    public struct RandomComponent : IComponentData {
        public int Seed;
        public Random Random;
    }
}