using Unity.Collections;
using Unity.Entities;

namespace ToggleBehaviour.Components {
    public struct TeamSelectedStateComponent : IComponentData {
        public Team Team;
    }
    
    public struct TeamMemberComponent : IComponentData {
        public Team Team;        
    }
    
    public struct IsSelectedComponent : IComponentData, IEnableableComponent {
    }
    
    public readonly struct PlayerNameComponent : IComponentData {
        public readonly FixedString32Bytes PlayerNameValue;
 
        public PlayerNameComponent(string name) {
            PlayerNameValue = new FixedString32Bytes(name);
        }
    }
    
    public enum Team {
        Red,
        Blue
    }
}