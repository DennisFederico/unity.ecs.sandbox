using TowerDefenseBase.Aspects;
using TowerDefenseBase.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace TowerDefenseBase.Systems {
    
    /// <summary>
    /// Aspects cannot work with Managed objects, so we use this system to sync the visual counterparts of the Entities to their GameObjects.
    /// </summary>
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(TransformSystemGroup))]
    [UpdateAfter(typeof(MoveByAspectSystem))]
    public partial struct SyncVisualGameObjectSystem : ISystem {
        private static readonly int SpeedAnimHash = Animator.StringToHash("speed");

        private EntityQuery _entitiesToSync;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            _entitiesToSync = SystemAPI.QueryBuilder()
                .WithAspect<PathFollowingAspect>()
                .WithAll<VisualTransformComponent>()
                .WithAll<VisualAnimatorComponent>()
                .Build();
            //Update the system only if there are entities to sync
            state.RequireForUpdate(_entitiesToSync);
        }
        
        [BurstDiscard]
        public void OnUpdate(ref SystemState state) {
            foreach (var (pathFollower, vTransform, vAnimator) in SystemAPI.Query<PathFollowingAspect, VisualTransformComponent, VisualAnimatorComponent>()) {
                vTransform.Transform.position = pathFollower.Position;
                vTransform.Transform.rotation = pathFollower.Rotation;
                vAnimator.Animator.SetFloat(SpeedAnimHash, pathFollower.Speed);
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }
}