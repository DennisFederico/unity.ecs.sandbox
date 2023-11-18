using Crowds.Aspects;
using Crowds.Components;
using Unity.Entities;

namespace Crowds.Systems {
    public partial class MovingSystem : SystemBase {
        protected override void OnUpdate() {
            
            //The idiomatic foreach is the preferred choice with Entities 1.0, but it will run in the Main thread
            // foreach (var (transform, speed, targetPosition) in SystemAPI.Query<RefRW<LocalTransform>, Speed, RefRO<TargetPosition>>()) {
            //     //Calculate the direction to the target
            //     var direction = math.normalize(targetPosition.ValueRO.Value - transform.ValueRO.Position);
            //     transform.ValueRW.Position += direction * (speed.Value * SystemAPI.Time.DeltaTime);
            // }

            var randomComponent = SystemAPI.GetSingletonRW<RandomComponent>();
            foreach (var moveToPositionEntities in SystemAPI.Query<MoveToPositionAspect>()) {
                moveToPositionEntities.Move(SystemAPI.Time.DeltaTime, randomComponent);
            }
        }
    }
}