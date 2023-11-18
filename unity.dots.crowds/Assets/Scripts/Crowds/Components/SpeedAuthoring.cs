using Unity.Entities;
using UnityEngine;

namespace Crowds.Components {
    public class SpeedAuthoring : MonoBehaviour {
        [SerializeField] private float speed = 1f;

        private class SpeedAuthoringBaker : Baker<SpeedAuthoring> {
            public override void Bake(SpeedAuthoring authoring) {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Speed { Value = authoring.speed });

                // var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
                // var entity2 = entityManager.CreateEntity(typeof(Speed));
                // entityManager.SetComponentData(entity2, new Speed {Value = authoring.speed});
                
                // var add1 = CreateAdditionalEntity(TransformUsageFlags.Dynamic, false, entityName: "Another 1");
                // AddComponent(add1, new Speed { Value = authoring.speed + 100 });
                
                // var add2 = CreateAdditionalEntity(TransformUsageFlags.Dynamic, true, entityName: "Another 2");
                // AddComponent(add2, new Speed { Value = authoring.speed + 200 });
            }
        }
    }
}