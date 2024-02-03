using Unity.Collections;
using Unity.Entities;

namespace Switching.Components {
    
    public struct PlayerNameComponent : IComponentData {
        public FixedString32Bytes PlayerNameValue;
    }
}