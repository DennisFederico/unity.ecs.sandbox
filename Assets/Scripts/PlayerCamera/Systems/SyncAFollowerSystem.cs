using Unity.Entities;
using Unity.Transforms;

namespace PlayerCamera.Systems {
    
    public partial struct SyncAFollowerSystem : ISystem {
        //Cannot use Burst
        public void OnUpdate(ref SystemState state) {
            foreach (var (followerId, transform) in 
                     SystemAPI.Query<RefRO<SyncAFollower>, RefRO<LocalTransform>>()) {
                //This approach is inefficient if there are many follow:followers given the dual loop
                //Could use a HashMap instead of a List to improve performance
                foreach (var follower in SyncMeToFollowEntity.Followers) {
                    if (followerId.ValueRO.FollowerId == follower.myId) {
                        if (follower.position)
                            follower.transform.position = transform.ValueRO.Position;
                        if (follower.rotation)
                            follower.transform.rotation = transform.ValueRO.Rotation;
                        break;
                    }
                }
            }
        }
    }
}