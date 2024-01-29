using Collider.Components;
using Unity.Entities;
using UnityEngine;

namespace Collider.Authoring {
    public class SpheresHolderAuthoring : MonoBehaviour {
        
        [SerializeField] private GameObject leftClickSphere;
        [SerializeField] private GameObject rightClickSphere;
        [SerializeField] private GameObject middleClickSphere;
        private class SpheresHolderAuthoringBaker : Baker<SpheresHolderAuthoring> {
            public override void Bake(SpheresHolderAuthoring authoring) {
                var holderEntity = GetEntity(TransformUsageFlags.None);
                AddComponent(holderEntity, new SpheresHolderComponent {
                    LeftClickSphere = GetEntity(authoring.leftClickSphere, TransformUsageFlags.Dynamic),
                    RightClickSphere =  GetEntity(authoring.rightClickSphere, TransformUsageFlags.Dynamic),
                    MiddleClickSphere =  GetEntity(authoring.middleClickSphere, TransformUsageFlags.Dynamic),
                });
            }
        }
    }
}