using Unity.Entities;

namespace ToggleBehaviour.Components {
    
    public enum Team {
        Red,
        Blue
    }
    
    public struct TeamMemberComponent : IComponentData {
        public Team Team;        
    }
    
    public struct TeamSelectedStateComponent : IComponentData {
        public Team Team;
    }
}