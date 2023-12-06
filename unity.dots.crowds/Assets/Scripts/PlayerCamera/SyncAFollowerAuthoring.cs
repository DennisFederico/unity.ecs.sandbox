using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace PlayerCamera {
    
    public struct SyncAFollower : IComponentData {
        public FixedString32Bytes FollowerId;
    }
    public class SyncAFollowerAuthoring : MonoBehaviour {
        public string followerId;
        private class SyncAFollowerAuthoringBaker : Baker<SyncAFollowerAuthoring> {
            public override void Bake(SyncAFollowerAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new SyncAFollower {
                    FollowerId = authoring.followerId
                });
            }
        }
    }
}