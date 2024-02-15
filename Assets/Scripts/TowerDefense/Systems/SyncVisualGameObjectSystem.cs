using TowerDefense.Aspects;
using TowerDefense.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;

namespace TowerDefense.Systems {
    
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(TransformSystemGroup))]
    [UpdateAfter(typeof(MoveByAspectSystem))]
    //THIS SYSTEM IS REQUIRED BECAUSE ASPECTS CANNOT HAVE MANAGED COMPONENTS
    public partial struct SyncVisualGameObjectSystem : ISystem {
        private static readonly int SpeedAnimHash = Animator.StringToHash("speed");

        private EntityQuery _entitiesToSync;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            _entitiesToSync = SystemAPI.QueryBuilder()
                .WithAspect<PathFollowerAspect>()
                .WithAll<VisualTransformComponent>()
                .WithAll<VisualAnimatorComponent>()
                .Build();
            state.RequireForUpdate(_entitiesToSync);
            
        }
        
        [BurstDiscard]
        public void OnUpdate(ref SystemState state) {
            foreach (var (pathFollower, vTransform, vAnimator) in 
                     SystemAPI.Query<PathFollowerAspect, VisualTransformComponent, VisualAnimatorComponent>()) {
                vTransform.Transform.position = pathFollower.Position;
                vTransform.Transform.rotation = pathFollower.Rotation;
                vAnimator.Animator.SetFloat(SpeedAnimHash, pathFollower.Speed);
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }
}