using Unity.Burst;
using Unity.Entities;

namespace Recap101.Systems {
    public partial struct CleanVisualGameObjectSystem : ISystem {
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) { }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }
}