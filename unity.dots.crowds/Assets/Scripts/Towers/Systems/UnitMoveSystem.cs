using Towers.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Towers.Systems {
    
    //THIS SYSTEM SHOULD ONLY WORK ON ENABLED "MoveComponent" COMPONENTS
    //THE COMPONENT SHOULD BE DISABLED ONCE THE TARGET POSITION IS REACHED
    //CHANGE THE UNIT FORMATION SYSTEM TO ENABLE THE COMPONENT AND SET THE TARGET POSITIONS WHEN THE FORMATION CHANGES
    [DisableAutoCreation]
    public partial struct UnitMoveSystem : ISystem {
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            
            foreach (var (transform, moveData) in 
                SystemAPI.Query<RefRW<LocalTransform>, RefRW<MoveComponent>>()) {
                
                //Debug.Log($"Is Enabled: {entity.Index} {enabled.ValueRO}");
                
                //if (!enabled.ValueRO) continue;

                var distance = moveData.ValueRO.TargetPosition - transform.ValueRO.Position;

                //Are we close enough to the target
                if (math.length(distance) < 0.25f) {
                    transform.ValueRW.Position = moveData.ValueRO.TargetPosition;
                    //enabled.ValueRW = false;
                    continue;
                }
                
                //Approach the target position
                transform.ValueRW.Position += math.normalize(distance) * (moveData.ValueRO.Speed * SystemAPI.Time.DeltaTime);
                
                //Face the target position
                //transform.ValueRW.Rotation = quaternion.LookRotationSafe(math.normalize(distance), math.up());
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }
}