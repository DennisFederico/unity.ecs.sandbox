using Unity.Entities;
using UnityEngine;

namespace TowerDefenseBase.Components {

    /// <summary>This component is a class because it holds a reference to a managed object
    /// The GameObject prefab of the visual counterpart in the monobehaviour world
    /// </summary>
    public class VisualGameObjectComponent : IComponentData {
        public GameObject VisualPrefab;
    }
}