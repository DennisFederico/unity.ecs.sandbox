using Unity.Collections;
using Unity.Entities;

namespace ToggleBehaviour.Components {
    public readonly struct PlayerNameComponent : IComponentData {
        public readonly FixedString32Bytes PlayerNameValue;
 
        public PlayerNameComponent(string name) {
            PlayerNameValue = new FixedString32Bytes(name);
        }
    }
}