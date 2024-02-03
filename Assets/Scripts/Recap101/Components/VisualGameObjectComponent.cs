using Unity.Entities;
using UnityEngine;

namespace Recap101.Components {

    public class VisualGameObjectComponent : IComponentData {
        public GameObject VisualPrefab;
    }
}