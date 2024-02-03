using Unity.Entities;
using UnityEngine;

namespace TowerDefense.Components {
    public class VisualTransformComponent : ICleanupComponentData {
        public Transform Transform;        
    }
}