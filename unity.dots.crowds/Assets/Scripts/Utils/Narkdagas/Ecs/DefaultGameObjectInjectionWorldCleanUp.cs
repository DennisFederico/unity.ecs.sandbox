using Unity.Entities;
using UnityEngine;

namespace Utils.Narkdagas.Ecs {
    public class DefaultGameObjectInjectionWorldCleanUp : MonoBehaviour {
        private void OnDestroy() {
            if (World.DefaultGameObjectInjectionWorld is not { IsCreated: true }) return;
            var worldName = World.DefaultGameObjectInjectionWorld.Name;
            World.DefaultGameObjectInjectionWorld.Dispose();
            var world = new World(worldName);
            World.DefaultGameObjectInjectionWorld = world;
            var systems = DefaultWorldInitialization.GetAllSystems(WorldSystemFilterFlags.Default);
            DefaultWorldInitialization.AddSystemsToRootLevelSystemGroups(world, systems);
            ScriptBehaviourUpdateOrder.AppendWorldToCurrentPlayerLoop(world);
        }
    }
}