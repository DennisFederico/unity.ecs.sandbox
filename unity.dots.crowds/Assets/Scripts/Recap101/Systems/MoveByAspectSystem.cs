using Recap101.Aspects;
using Unity.Burst;
using Unity.Entities;

namespace Recap101.Systems {
    public partial struct MoveByAspectSystem : ISystem {
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            
            foreach (var moveByAspect in SystemAPI.Query<PathFollowerAspect>()) {
                moveByAspect.FollowPath(SystemAPI.Time.DeltaTime);
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }
}