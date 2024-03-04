using Unity.Entities;
using UnityEngine;

namespace TowerDefenseBase.Components {
    
    /// <summary>This component is a class because it holds a reference to a managed object
    /// The Animator component of the visual counterpart in the monobehaviour world
    /// </summary>
    public class VisualAnimatorComponent : IComponentData {
        public Animator Animator;
    }
}