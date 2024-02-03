using Unity.Entities;
using UnityEngine;

namespace TowerDefense.Components {

    public class VisualGameObjectComponent : IComponentData {
        public GameObject VisualPrefab;
    }
}