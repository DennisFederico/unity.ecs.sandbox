using Crowds.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;
using Unity.Jobs;

namespace Crowds.Systems.Jobs {
    
    [BurstCompile]
    public partial struct CreateCrowdMemberJob : IJobParallelFor {
        public EntityCommandBuffer.ParallelWriter Ecb;
        [ReadOnly] public Entity PrefabEntity;
        // [ReadOnly] public float Speed;
        // [ReadOnly] public float3 TargetPosition;
        
        // [NativeDisableUnsafePtrRestriction]
        // public RefRW<RandomComponent> RandomComponent;
        
        public void Execute(int index) {
            var entityInstance = Ecb.Instantiate(index, PrefabEntity);
            //Ecb.SetComponent(index, entityInstance, new Speed() { ParentEntity = RandomComponent.ValueRW.ParentEntity.NextFloat(1f, 3f) });
            //Ecb.SetComponent(index, entityInstance, new TargetPosition() { ParentEntity = Utils.Utils.NewRandomPosition(RandomComponent.ValueRW.ParentEntity) });
        }
    }
}