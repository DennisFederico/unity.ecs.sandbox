using PlayerCamera.Components;
using Unity.Burst;
using Unity.Entities;
using Unity.Physics;

namespace PlayerCamera.Systems {
    public partial struct FreezeRotationSystem : ISystem {
        [BurstCompile]
        public void OnCreate(ref SystemState state) {
            
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            foreach (var (flags, mass) in SystemAPI.Query<
                         RefRO<FreezeRotationComponentData>, 
                         RefRW<PhysicsMass>>()) {
                if (flags.ValueRO.Flags.x) mass.ValueRW.InverseInertia.x = 0;
                if (flags.ValueRO.Flags.y) mass.ValueRW.InverseInertia.y = 0;
                if (flags.ValueRO.Flags.z) mass.ValueRW.InverseInertia.z = 0;
            }
        }

        [BurstCompile]
        public void OnDestroy(ref SystemState state) { }
    }
}