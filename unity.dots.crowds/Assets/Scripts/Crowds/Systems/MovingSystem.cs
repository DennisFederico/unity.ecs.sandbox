using Crowds.Aspects;
using Crowds.Components;
using Unity.Entities;

namespace Crowds.Systems {
    public partial class MovingSystem : SystemBase {
        protected override void OnUpdate() {
            //DISABLED: This is the old way of doing things, it will run in the Main thread
            if (true) return;

            //Singleton Component containing the Random object
            // var randomComponent = SystemAPI.GetSingletonRW<RandomComponent>();
            //
            // Entities
            //     .WithName("Moving_System")
            //     //.WithAll<MoveToPositionAspect>()
            //     .ForEach((MoveToPositionAspect moveToPositionAspect) => { moveToPositionAspect.Move(SystemAPI.Time.DeltaTime); })
            //     .ScheduleParallel();
            //
            // Entities
            //     .WithName("ReachTargetPosition_System")
            //     //.WithAll<MoveToPositionAspect>()
            //     .ForEach((MoveToPositionAspect moveToPositionAspect) => { moveToPositionAspect.TestReachedTargetPosition(randomComponent); })
            //     .ScheduleParallel();

            // foreach (var moveToPositionEntities in SystemAPI.Query<MoveToPositionAspect>()) {
            //     moveToPositionEntities.Move(SystemAPI.Time.DeltaTime);
            //     moveToPositionEntities.TestReachedTargetPosition(randomComponent);
            // }
        }
    }
}