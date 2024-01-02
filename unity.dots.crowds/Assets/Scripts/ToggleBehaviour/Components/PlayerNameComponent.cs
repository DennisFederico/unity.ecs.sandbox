using Unity.Collections;
using Unity.Entities;

namespace ToggleBehaviour.Components {
    
    public struct PlayerNameComponent : IComponentData {
        public FixedString32Bytes PlayerNameValue;
    }
}