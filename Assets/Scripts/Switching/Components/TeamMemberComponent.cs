using Unity.Entities;
using Unity.Mathematics;

namespace Switching.Components {
    
    public enum Team {
        Red,
        Blue,
        None
    }
    
    public struct TeamMemberComponent : IComponentData {
        public Team Team;

        public float4 Color {
            get {
                switch (Team) {
                    case Team.Red:
                        return new float4(1, 0, 0, 1);
                    case Team.Blue:
                        return new float4(0, 0, 1, 1);
                    default:
                        return new float4(0.5f, 0.5f, 0.5f, 1);
                }
            }
        }
    }
    
    public struct TeamSelectedStateComponent : IComponentData {
        public Team Team;
    }
}