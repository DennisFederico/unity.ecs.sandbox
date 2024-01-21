using TowerDefense.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;

namespace TowerDefense.Systems {
    
    public partial struct SyncVisualGameObjectSystem : ISystem {
        [BurstCompile]
        public void OnCreate(ref SystemState state) {

        }
        
        public void OnUpdate(ref SystemState state) {
            foreach (var (transform, speed, vTransform, vAnimator) in SystemAPI.Query<RefRO<LocalTransform>, RefRO<MoveSpeedComponent>, VisualTransformComponent, VisualAnimatorComponent>()) {
                vTransform.Transform.position = transform.ValueRO.Position;
                vTransform.Transform.rotation = transform.ValueRO.Rotation;
                vAnimator.Animator.SetFloat("speed", speed.ValueRO.Value);
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }
}