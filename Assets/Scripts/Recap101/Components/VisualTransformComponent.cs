using Unity.Entities;
using UnityEngine;

namespace Recap101.Components {
    public class VisualTransformComponent : ICleanupComponentData {
        public Transform Transform;        
    }
}