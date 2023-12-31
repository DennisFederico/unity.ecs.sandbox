using Unity.Entities;
using Unity.Mathematics;

namespace ToggleBehaviour.Components {
    
    public enum Team {
        Red,
        Blue
    }
    
    public struct TeamMemberComponent : IComponentData {
        public Team Team;
        
        public float4 Color => Team == Team.Blue ? new float4(0, 0, 1, 1) : new float4(1, 0, 0, 1);
    }
    
    public struct TeamSelectedStateComponent : IComponentData {
        public Team Team;
    }
}