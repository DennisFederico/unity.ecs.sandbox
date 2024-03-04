using Unity.Entities;
using UnityEngine;

namespace TowerDefenseBase.Components {
    
    /// <summary>This component is a class because it holds a reference to a managed object
    /// The Transform (GameObject) og the visual counterpart in the monobehaviour world
    /// </summary>
    public class VisualTransformComponent : ICleanupComponentData {
        public Transform Transform;        
    }
}