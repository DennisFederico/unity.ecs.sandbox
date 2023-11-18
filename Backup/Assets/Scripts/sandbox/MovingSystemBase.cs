using Unity.Entities;

namespace sandbox {
    public partial class MovingSystemBase /*: SystemBase*/ {
        // protected override void OnUpdate() {
        //     // Entities.ForEach((LocalTransform transformAspect, Speed speedAspect) => {
        //     //     transformAspect.Translate(new float3(speedAspect.Value * SystemAPI.Time.DeltaTime, 0, 0));
        //     // }).ScheduleParallel();
        //     foreach (var (transformAspect, speed, targetPosition) in SystemAPI.Query<RefRW<LocalTransform>, Speed, RefRW<TargetPosition>>()) {
        //         //Direction
        //         float3 direction = math.normalize(targetPosition.ValueRW.Value - transformAspect.ValueRW.Position);
        //         //Move
        //         transformAspect.ValueRW.Position += direction * (SystemAPI.Time.DeltaTime * speed.Value);
        //     }
        // }
        
        // protected override void OnUpdate() {
        //     var randomRef = SystemAPI.GetSingletonRW<RandomComponent>();
        //     foreach (var moveToPosition in SystemAPI.Query<MoveToPositionAspect>()) {
        //         moveToPosition.DoMove(SystemAPI.Time.DeltaTime);
        //         moveToPosition.TestTargetPosition(randomRef);
        //     }
        // }
    }
}