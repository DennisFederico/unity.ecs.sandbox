using Selection.Components;
using Unity.Entities;
using Unity.Physics.Authoring;
using UnityEngine;

namespace Selection.MonoBehaviours {
    public class SelectionColliderTagAuthoring : MonoBehaviour {
        [SerializeField] private bool additive;
        [SerializeField] private PhysicsCategoryTags belongsTo;
        [SerializeField] private PhysicsCategoryTags collidesWith;
        private class SelectionColliderTagAuthoringBaker : Baker<SelectionColliderTagAuthoring> {
            public override void Bake(SelectionColliderTagAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new SelectionColliderDataComponent {
                    Additive = authoring.additive,
                    BelongsTo = authoring.belongsTo,
                    CollidesWith = authoring.collidesWith
                } );
            }
        }
    }
}