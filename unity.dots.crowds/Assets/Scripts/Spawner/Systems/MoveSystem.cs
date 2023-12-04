using Spawner.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Spawner.Systems {
    [DisableAutoCreation]
    public partial struct MoveSystem : ISystem {
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var forward = Input.GetAxis("Vertical") * SystemAPI.Time.DeltaTime;
            var rotate = Input.GetAxis("Horizontal") * SystemAPI.Time.DeltaTime;
            
            //Use a query to move all the entities with the proper component data
            foreach (var (entity, transform) in SystemAPI.Query<RefRO<MovingComponentData>, RefRW<LocalTransform>>()) {
                transform.ValueRW.Position += transform.ValueRW.Forward() * forward * entity.ValueRO.moveSpeed;
                transform.ValueRW.Rotation = math.mul(quaternion.RotateY(rotate * entity.ValueRO.rotateSpeed), transform.ValueRO.Rotation);
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }
}