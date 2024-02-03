using Unity.Entities;

namespace Collider.Components {
    public struct SpheresHolderComponent : IComponentData {
        public Entity LeftClickSphere;
        public Entity RightClickSphere;
        public Entity MiddleClickSphere;
        
        public Entity GetPrefabForClick (int buttonClicked) => buttonClicked switch {
            0 => LeftClickSphere,
            1 => RightClickSphere,
            2 => MiddleClickSphere,
            _ => Entity.Null
        };
    }
}