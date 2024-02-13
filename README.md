# Unity ECS Sandbox

This is a sandbox project for Unity's Entity Component System (ECS) and the C# Job System.
It is composed of a few simple sample scenes with some basics ***tests*** of Unity's ECS, and for me to experiment with the new system and use as a reference for future projects.

**NOTE:** This is a work in progress and it's not meant to be a tutorial, a complete project, or a collection of Best Practices, but a bunch of simple scenes to test and learn how to use the Unity ECS Model Api.

**NOTE 2**: Some of these scenes are based or inspired by different sources, including Unite talks about ECS, [Code Monkey](https://unitycodemonkey.com/) (Hugo, I'm a big fan), [TurboMakesGames](https://www.youtube.com/c/TurboMakesGames) and [Wayn Games](https://www.youtube.com/@WAYNGames) videos... (I hope you guys get to check this project at some point :) ).

## Dependencies
- **com.unity.entities**: 1.0.16
- **com.unity.physics**: 1.0.16
- **com.unity.entities.graphics**: 1.0.16

## Getting Started

Download or clone the repository and open the project with Unity. I've used 2023.1.20f1 since I've been having problems with 2023.2.xx.

## Scenes

The code for each scene is contained in their own namespace in the `Assets/Scripts` folders, with the exception of some imported utils from [Code Monkey](https://unitycodemonkey.com/) for managing the grid of A*Pathfinding, other utils of my own.  


### 1. Bootstrap & MainMenu

The bootstrap it's just a "launcher" to load the scene loader and scene transitions manager, the main menu scene is loaded from here and it's a simple scene with a few buttons to load each test/demo scene in the Sandbox.

---

### 2. Formation Change Demo
([Assets/Scripts/Formations](Assets/Scripts/Formations) namespace)

Here we have 2 Baked "Towers" (Entities) that controls the number of units on their armies and the formation they should follow around them.

The TowerComponent holds the number of "units" on the tower armies (cubes in the scene), the army formation and the radius around the tower. The formation is a simple enum with 3 values: Line, Circle, and Square.

Note from this setup, that the actual formation resides in the Tower, which is checked by the UnitChangeFormationSystem, to calculate the new unit position. And the reference to the "Parent" Tower is held by the an ISharedComponentData, so it's a "per chunk" component, and it's used to group the units that belong to the same tower.

#### Components
- **EntityPrefabComponent**: Holds the prefab of the "Unit" (cube) to be spawned by the SpawnUnitsSystem, the prefab and color is references in the **PrefabReferenceAuthoring** Baker.
- **TowerComponent**: Holds the number of units, the formation and the radius of the formation around the tower.
- **SpawnUnitsTag**: A simple *IEnableableComponent* tag used by the SpawnUnitsSystem to spawn the Tower armies.
- **FormationComponent**: Added to the "Units" (cubes) and holds the index in its army and the current formation.
- **MoveComponent**: Holds the target position for the units to move to, and the move speed. Used by the UnitMoveSystem.
- **ParentEntityReferenceComponent**: ISharedComponentData to hold the reference to the Tower Entity, all the cubes that "belong" to the same tower will be in a specific ***chunk***, like grouping per "parent" tower, even if they have the same resulting archetype, since ISharedComponentData is a "per chunk" component, this can be particular useful when using IJobChunk and IJobEntities (since it used IJobChunk under the hood) .
  (see. https://docs.unity3d.com/Packages/com.unity.entities@1.0/manual/components-shared-introducing.html)

#### Systems
- **SpawnUnitsSystem** checks for a **IEnableableComponent** SpawnUnitsTag, and if it's enabled, it will spawn the units around the tower in the formation specified in the TowerComponent. It also assigns the cube color using *URPMaterialPropertyBaseColor* component.
- **UnitMoveSystem** moves the units to the target position specified in the MoveComponent.
- **ChangeFormationSystem**, this system *Update* method runs every 3 seconds approx. It Queries for TowerComponent with **no** SpawnUnitsTag (disabled), meaning that their units have already spawned and changes the formation value of the Tower (not the units).
- **UnitChangeFormationSystem** queries for all *FormationComponent* and the *ParentEntityReferenceComponent*, if the unit and the tower formation are different, ***a new position is calculated based on the formation dictated by the tower***. 

#### Others
- ***PositionUtils*** class contain helper methods to calculate the position of the units based on the formation and the tower position.

![Formations.gif](webimg%2FFormations.gif)

---

### 3. Team/Color Switch Demo
([Assets/Scripts/Switching](Assets/Scripts/Switching) namespace)

In this scene there are 2 "teams" colored blue and red, the selected teams changes with the click of the left mouse button,
when that happens the members of the team switch color to their team color if "selected" or become neutrally colored when not.

Here was my first test for [URPMaterialPropertyBaseColor](https://docs.unity3d.com/Packages/com.unity.entities.graphics@1.0/manual/material-overrides.html) Component to change the color of the entities.
And Scheduling jobs (IJobEntity) with EntityQueries as argument. Also the "selection" visual is added/removed as a child of the selected entity, so in case the parent is moved around, such visual should follow the entity as part of the hierarchy.

This test does a heavy use of tag components to mark the entities as they switch states, this tags are **IEnableableComponent**, so the tags are not added/removed, just enabled/disabled, to avoid structural changes.
But there are other "structural changes" in this exercise, like adding/removing the "selection visual" as a child of the selected entities.

#### Components

- **IsSelectedComponentTag**: A simple IEnableableComponent to mark the entities that are "selected".
- **IsPlayingComponentTag**: (not really used) IEnableableComponent to mark the entities that are "playing" or "benched".
- **PlayerNameComponent**: (not really used) Holds the name of the player, just to show that you can add any kind of data to the entities. Ideally this could be used to show the value when hovering or clicking over the entity visual.
- **PrefabHolderComponent**: This one holds the prefab of the ***selection visual*** that is instantiated for each selected entity and added to its hierarchy.
- **TeamMemberComponent**: Holds the Team and returns the Color as a ***float4*** value depending on the Team.
- **TeamSelectedStateComponent**: Only used during the "start" phase of the *SwitchTeamSelectionSystem*, it dictates which is the team to be selected on the first (switch) mouse click.
- **VisualComponentTag**: A tag to mark the "selection visual" entities, so they can be removed when the entity is deselected. 
- **VisualRepresentationTag**: A tag to represent the entities that have a visual representation in the scene, and subject to "color change", in the context of this exercise it would have been the "playing" (not benched) entities.
- **DebugLogDataComponent**: A simple component to hold the message to be logged by the DebugLogSystem.

#### Systems

- **SwitchTeamSelectionSystem**: This system is responsible for the "switch" of the selected team, it checks for left-mouse clicks and process the changes, adding/removing the selection visual, changing color, etc. ***Using jobs (IJobEntity) and EntityQueries***.
- **StateEventDispatcherSystem**: This is and "event" SystemBase, it counts the selected entities for the currently selected team and dispatches and event if it is different from the previous "frame".
  - ***CanvasSwitchSelectionTeamHandler*** is a MonoBehaviour that listens to the event and updates the UI to show the number of selected entities for each team.
```csharp
        private void OnEnable() {
            _world = World.DefaultGameObjectInjectionWorld;
            if (_world.IsCreated) {
                _stateEventDispatcherSystem = _world.GetOrCreateSystemManaged<StateEventDispatcherSystem>();
                _stateEventDispatcherSystem.SimulationStateChangedEvent += OnSimulationStateChangedEvent;
            }
        }
```

- **DebugLogSystem**: A SystemBase created as prototype of an event system that dispatched log messages from other systems. A MonoBehaviour can register to the event and log the messages to the console or any other Text output.

![TeamSwitch.gif](webimg%2FTeamSwitch.gif)

---

### 4. Pathfinding (MonoBehaviour) Demo
([Assets/Scripts/Utils/Narkdagas/PathFinding](Assets/Scripts/Utils/Narkdagas/PathFinding) namespace)

This is an exercise based on the great videos from [Code Monkey](https://unitycodemonkey.com/) tutorial on A* Pathfinding, using C# Job System. The original tutorial is [here](https://www.youtube.com/watch?v=1bO1FdEThnU).

![PathJobs.gif](webimg%2FPathJobs.gif)

### 5. Pathfinding (ECS) Demo
([Assets/Scripts/AStar](Assets/Scripts/AStar) namespace)

This is a self-made conversion of the previous Pathfinding exercise to use Unity's ECS. It demonstrates a series of systems chained that:
creates new entities, calculate a path for them to follow, moves them and request, set a new destination upon reaching the end of the path, and back to calculate a new path.

Entities are spawned by the *CreatePathFollowerSystem* when processing a Buffer of *CreateNewPathFollowerRequest* components, this process creates a new entity from a prefab
with a *PathFindingRequest* component that holds the start and end positions for the ***PathfindingSystem***.

The PathfindingSystem queries for Entities with enabled *PathFindingRequest* components, and disabled PathFollowIndex component, this last component means that the entity is
already following a path, so it's not a candidate for pathfinding.

```csharp
            _entitiesWithPathRequest = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<PathFindingUserTag>()
                .WithAll<PathFindingRequest>()
                .WithDisabledRW<PathFollowIndex>()
                .WithAllRW<PathPositionElement>()
                .Build(ref state);
```

A path is calculated by the *FindPathForEntityJob* that is scheduled using the above query, the job is called along with the current grid as a NativeArray of **PathNode** that contains the info (obstacles, etc.).
```csharp
        public void OnUpdate(ref SystemState state) {
            
            var gridInfo = SystemAPI.GetSingleton<GridSingletonComponent>();
            var grid = SystemAPI.GetSingletonBuffer<PathNode>(true);
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            state.Dependency = new FindPathForEntityJob {
                GridInfo = gridInfo,
                Grid = grid.AsNativeArray(),
                Ecb = ecb.AsParallelWriter(),
            }.ScheduleParallel(_entitiesWithPathRequest, state.Dependency);
        }
```

The path is stored as a Buffer of **PathPositionElement** on the entity and a **PathFollowIndex** component holds the index of the path that the entity is currently moving to, 
this is used by the *MovePathFollowerSystem* that updates the LocalTransform of the entity.

The whole **Grid** is stored as a SingletonComponent on ECS by a MonoBehavior, marking/removing obstacles is handled by **PathfindingEcsGridMono** that keeps a "parallel" copy of the grid
and writes it back to ECS on changes. (Not the most performant approach, but works fine for small grids, a better approach would have been to "push" the grid updates on a buffer and process them on an GridUpdateSystem)

```csharp
        private void InjectGridIntoEcsSystem() {
            if (_world.IsCreated) {
                _theGrid = _world.EntityManager.CreateSingleton(new GridSingletonComponent {
                    Width = width,
                    Height = height,
                    CellSize = cellSize,
                    Origin = transform.position
                },"TheGrid");
                
                _world.EntityManager.AddComponent<PathNode>(_theGrid);
                var ecsGrid = _world.EntityManager.GetBuffer<PathNode>(_theGrid);
                ecsGrid.AddRange(_grid.GetGridAsArray(Allocator.Temp));
            } 
        }
```

```csharp
        private void GridOnGridValueChanged(object sender, OnGridValueChangedEventArgs e) {
            //TODO: Notify the system about changes in the grid instead of replacing the whole grid.
            if (_world.IsCreated && _world.EntityManager.Exists(_theGrid)) {
                var ecsGrid = _world.EntityManager.GetBuffer<PathNode>(_theGrid);
                ecsGrid.CopyFrom(_grid.GetGridAsArray(Allocator.Temp));
                ecsGrid.TrimExcess();
            } 
        }
```

### Components
- **CreateNewPathFollowerRequest**: A buffer element to spawn a new entity from a prefab.
- **PathFindingRequest**: An IEnableableComponent that signals the PathfindingSystem to calculate a new path.
- **GridSingletonComponent**: Holds the grid info, width, height, cell size, and origin vector.
- **MoveSpeed**: Holds the speed of the entity when following the path.
- **PathPositionElement**: A buffer element that holds the path positions.
- **PathFollowIndex**: Holds the index of the current path position.
- etc...

### Systems
- **CreatePathFollowerSystem**: Spawns a new entity from a prefab with a PathFindingRequest component.
- **PathfindingSystem**: Calculates the path for the entities with enabled PathFindingRequest component.
- **MovePathFollowerSystem**: Moves the entities (with enabled **PathFollowIndex**) to the next position in the path. On reaching the end of the path, it disables the **PathFollowIndex** component.
- **NewRandomPathRequestSystem**: For entities with disabled **PathFollowIndex** and disabled **PathFindingRequest**, calculates a Random Target position and enables the **PathFindingRequest** component to calculate a new path to that position.

![Pathfind-ecs2.gif](webimg%2FPathfind-ecs2.gif)

### 6. Load Systems Programatically
--TODO Description

![SystemStartStop.gif](webimg%2FSystemStartStop.gif)

### 7. Click and Box Selection
---TODO Description

![ClickSelect.gif](webimg%2FClickSelect.gif)

### 8. Spawner System
--TODO Description

![Crowds.gif](webimg%2FCrowds.gif)

### 9. Physics Trigger with Particle FX
--TODO Description

![TriggerFx.gif](webimg%2FTriggerFx.gif)

### 10. Swarm Magnet Scene
--TODO Description

![Swarm.gif](webimg%2FSwarm.gif)

### 11. Tower Defense Scene
--TODO Description

![Towers.gif](webimg%2FTowers.gif)




