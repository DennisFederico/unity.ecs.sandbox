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

The code for each scene is contained in their own namespace in the `Assets/Scripts` folders, with the exception of some imported utils from [Code Monkey](https://unitycodemonkey.com/) for managing the grid of A*Pathfinding, and other utils of my own.  

- [Formation Change](#2-formation-change-demo)
- [TeamColor Switch](#3-teamcolor-switch-demo)
- [Pathfinding Jobs Demo](#4-pathfinding-jobs-demo)
- [Pathfinding ECS Demo](#5-pathfinding-ecs-demo)
- [Load Systems Programatically](#6-load-systems-programatically)
- [Click and Box Selection](#7-click-and-box-selection)
- [Spawner System](#8-spawner-system)
- [Physics Trigger with Particle FX](#9-physics-trigger-with-particle-fx)
- [Swarm Magnet Scene](#10-swarm-magnet-scene)

---

### 1 Bootstrap & MainMenu

The bootstrap it's just a "launcher" to load the scene loader and scene transitions manager, the main menu scene is loaded from here and it's a simple scene with a few buttons to load each test/demo scene in the Sandbox.

---

### 2 Formation Change Demo
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

### 3 TeamColor Switch Demo
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

### 4 Pathfinding Jobs Demo
([Assets/Scripts/Utils/Narkdagas/PathFinding](Assets/Scripts/Utils/Narkdagas/PathFinding) namespace)

This is an exercise based on the great videos from [Code Monkey](https://unitycodemonkey.com/) tutorial on A* Pathfinding, using C# Job System. The original tutorial is [here](https://www.youtube.com/watch?v=1bO1FdEThnU).

![PathJobs.gif](webimg%2FPathJobs.gif)

### 5 Pathfinding ECS Demo
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

### 6 Load Systems Programatically
([Assets/Scripts/SystemLoader](Assets/Scripts/SystemLoader) namespace)

This is a simple scene to test the loading and unloading of systems programatically, it's a simple scene with balls that spawn randomly on a defined area on the system "update". 
The System is an ISystem struct and instead of using the derived SystemBase to "enable/disable" the System, for this test I've opted to add/remove the system from the World "System Update List".

The ***ActionMenuManager*** is the MonoBehaviour that adds and removes the ***SpawnBallSystem*** from the "System Update List" of the current ECS World, mimicking a "Start/Stop" of the system. 
A similar effect is achieved using the ***"RequireForUpdate"*** and using a singleton entity that can be created/destroyed (or enabling/disabling the component?). In this case the **BallSpawnerDataComponent** would be a perfect candidate. 

```csharp
        private void StopSystems() {
            if (_started && _world != null && _world.IsCreated) {
                // Debug.Log("Stopping Systems");
                var simulationSystemGroup = _world.GetExistingSystemManaged<SimulationSystemGroup>();
                var spawnBallSystemHandle = _world.GetExistingSystem<SpawnBallSystem>();
                simulationSystemGroup.RemoveSystemFromUpdateList(spawnBallSystemHandle);
                _world.DestroySystem(spawnBallSystemHandle);
                //TTL System destroys all TTL entities when destroyed
                var ttlSystem = _world.GetExistingSystem<TimeToLiveSystem>();
                simulationSystemGroup.RemoveSystemFromUpdateList(ttlSystem);
                _world.DestroySystem(ttlSystem);
            }

            _started = false;
        }
```

Another interesting thing of this exercise is the the ***RandomSeeder*** component used in a Singleton Entity to calculate any Random numbers needed by the systems.

#### Components
- **BallSpawnerDataComponent**: Holds the area where the balls are spawned and the spawn rate. This component is expected to be used in a singleton entity.
- **PrefabHoldingComponent**: Holds the prefab of the "Ball" to be spawned by the SpawnBallSystem.
- **RandomSeeder**: Holds the seed for the Random number generator, and is used by the SpawnBallSystem to calculate the position of the spawned balls.
- **TimeToLiveComponent**: Holds the time to live of the spawned balls, and is used by the TimeToLiveSystem to destroy the balls after the TTL is reached.

#### Systems
- **SpawnBallSystem**: Spawns the balls in the area defined by the BallSpawnerDataComponent.
- **TimeToLiveSystem**: Destroys the balls after the TTL is reached.

#### Others
- ***ActionMenuManager***: MonoBehaviour that listens to the UI buttons to "start/stop" the SpawnBallSystem.

![SystemStartStop.gif](webimg%2FSystemStartStop.gif)

---

### 7 Click and Box Selection
([Assets/Scripts/Selection](Assets/Scripts/Selection) namespace)

This scene is an exercise to apply the ideas from **Turbo Makes Games** video [Unity ECS Area Selection - RTS/City Builder - Unity DOTS Tutorial [ECS Ver. 0.17]](https://www.youtube.com/watch?v=n60pawK956A).
Where he implements a box selection using ECS physics via a ConvexCollider. Using the ConvexCollider we create a prism using four vertices calculated using the camera perspective, but care should be taken as additional
experiments have shown me that this vertices cannot be too far apart, or the collider will not be created, and the selection will not work. An improvement can be made to use another type of collider or calculate the
vertices in a different way, so that the bounding volume is not too big.

In the spirit of ECS, a data component (*SelectionVerticesBufferComponent*) is used by a system (*CreateSelectionPrismColliderSystem*) that creates a "physical" volume, the Physics World System then 
"triggers" collision based on *PhysicsCategoryTags* that are processed by the *MultipleUnitSelectionSystem*, this System processes the selection considering the *SelectedUnitTag*.

#### Components
- **RayCastBufferComponent**: A buffer element sent from MonoBehaviour, that holds the RaycastInput data for the PhysicsWorldSystem to calculate the hits when doing single click selection.
- **SelectionVerticesBufferComponent**: A buffer element sent from MonoBehaviour, that holds the vertices of the selection prism, used by the PhysicsWorldSystem to ***trigger*** the selection.
- **SelectionColliderDataComponent**: A data component added to the physical volume that contains the PhysicsCategoryTags of the expected trigger.
- **SelectedUnitTag** and **DecalComponentTag**: Tag components to mark the entities that are selected and selection visual entities.

#### Systems
- **CreateSelectionPrismColliderSystem**: Creates the physical volume using the vertices from the *SelectionVerticesBufferComponent*.
- **MultipleUnitSelectionSystem**: Processes the selection based on triggers created by the **PhysicsWorldSystem** between the physical volume and the capsules in the scene.
- **SingleUnitSelectionSystem**: Processes the selection based on RaycastInput data, it uses the **PhysicsWorldSystem** to calculate the hits and select the entities.
- **SelectedCountEventSystem**: A SystemBase that counts the entities with *SelectedUnitTag* and fires an event if the count is different from the previous count.

#### Others
- **UnitSelectManager**: MonoBehaviour that listens to the mouse input and sends the RaycastInput or the vertices of the selection prism to the ECS world, for single-click or box selection.
  - It also listen to *OnSelectedCountChanged* event from *SelectedCountEventSystem* to update the count of selected units.

![ClickSelect.gif](webimg%2FClickSelect.gif)

---

### 8 Spawner System
([Assets/Scripts/SimpleCrowdsSpawn](Assets/Scripts/SimpleCrowdsSpawn) namespace)

This is an interesting exercise that combines some of the previous exercises so far, with a mouse user "sends" a request to place an entity with a banner as a visual representation, 
and the "SpawnerSystem" will create entities from that position, once there's a spawn walking the scene, you can select one of them "at random" as the new spawn point. 

This exercise includes, PrefabHolder entity, SpawningSystem, MoveSystem, Randomness, Raycast, and Events (for the UI update of the unit count), 
similar on how it was done in the previous exercises.

The most important difference in this exercise is the use of [***Aspects***](https://docs.unity3d.com/Packages/com.unity.entities@1.0/manual/aspects-intro.html) as a wrapping structure that groups
access to common components of the moving entities (LocalTransform, Speed, TargetPosition).

Another "change" to this exercise is that the **selection marker** exists, and its position updated, in the MonoBehavior world; In previous exercises the marker was Instantiated from a 
a prefab on ECS side and added to the Hierarchy of the selected entity, thus its LocalTransform was updated from the "parent" entity. Not only the management of the position is required
from MonoBehavior side, but if the entity that holds the marker is destroyed, the MonoBehavior manager must "disable" the GameObject.

When a unit "selected" as SpawnPoint is "de-spawned" (destroyed), we lose the reference entity (as and Id) for spawning and the link from Mono to ECS,
to allow some "cleaning" processing after such entity with **SelectedMarker** component is destroyed, we extend the component from **ICleanupComponentData**, 
this helps the entity survive being destroyed but it gets stripped out of all its components with the exception of the *SelectedMarker*, then the **CleanSelectedMarkerSystem** 
finds such entity with **SelectedMarker** component but **No** LocalTransform and updates the reference in the *CrowdSpawner* singleton to `Entity.Null` 
(to avoid using an invalid entity for spawning) and removes the **SelectedMarker** component from the entity destroying it completely.
(see. [ICleanupComponentData](https://docs.unity3d.com/Packages/com.unity.entities@1.0/api/Unity.Entities.ICleanupComponentData.html) and [Cleanup Components](https://docs.unity3d.com/Packages/com.unity.entities@1.0/manual/components-cleanup.html) documentation)

#### Aspects
- **MoveToPositionAspect**: Wraps the LocalTransform, Speed, and TargetPosition components.
  - (ReadWrite) **LocalTransform**: The position and rotation of the entity.
  - (ReadOnly) **Speed**: The speed of the entity.
  - (ReadOnly) **TargetPosition**: The position to move to.

- **NewRandomPositionAspect**: Wraps the same components as the MoveToPositionAspect, but with different access pattern, fit to the purpose.
  - (ReadWrite) **LocalTransform**: The position and rotation of the entity.
  - (ReadOnly) **Speed**: The speed of the entity.
  - (ReadOnly) **TargetPosition**: The position to move to. 

#### Other Components
- **PlaceSpawnerRequestBuffer**: A buffer element to signal the placement of a new spawner (using the position, rotation) or selecting a random "CrowdMember" (Moving entity) as SpawnPoint. 
- **SpawnRequestBuffer**: A buffer element with an amount of entities to spawn by the *SpawnSystem* at the position of the *SelectedMarker* or * entity.
- **CrowdSpawner**: This component holds a reference to the prefab to Spawn and the currently selected entity as spawn point. Must be a singleton entity.

#### Systems
- **MoveSystem**: Does a "parallel" scheduling of two IJobEntity jobs, one to move the crowd members to the target position, 
and the other to select a new random target position for the crowd members that have reached the target position.
**NOTE**: IJobEntity parallelize at chunk level (it is an IJobChunk under the hood), so it's not running a thread per entity but per chunk, usually around 128 entities per chunk.
- **PlaceSpawnerSystem**: Processes the *PlaceSpawnerRequestBuffer* and sets the position and rotation of the "Banner" component or selects a CrowdMember at Random to be the Spawning point.
- **SpawnSystem**: Processes the *SpawnRequestBuffer* and spawns the entities at the position of the entity referenced by **CrowdSpawner** singleton.
- **CleanSelectedMarkerSystem**: Updates *CrowdSpawner* if its referenced spawning entity got deleted, by looking for entities with *SelectedMarker* but no LocalTransform.

#### Others
- **CrowdsSpawnerInoutManager**: MonoBehaviour that listens to the UI buttons to "place" the spawner or "select" a random crowd member as the spawn point.
- **SelectionMarkerManager**: MonoBehaviour that updates the position of the Selection Maker **GameObject** or disables it if the entity doesn't exist anymore.

![Crowds.gif](webimg%2FCrowds.gif)

---

### 9 Physics Trigger with Particle FX
([Assets/Scripts/Collider](Assets/Scripts/Collider) namespace)

Here is a simple test for Physics Trigger, and finding the "contact" point of the trigger to "spawn" a Particle FX.
In the scene the user can spawn balls that fall traversing a box collider and during the fall, contact points are 
picked at time intervals (triggers) to spawn some particle FX assigned to each ball prefab.

#### Components
- **SpheresHolderComponent**: Holds a prefab for 3 balls (one for each mouse button) that could be spawned by the *SpawnSphereSystem*. 
- **SpawnRequestComponentBuffer**: Is a buffer that holds which mouse button was clicked (int) the Ray from the camera and a distance over that ray 
on where to spawn the ball by the *SpawnSphereSystem*.
- **ImpactVfxComponent**: Holds the prefab of the particle FX to be spawned, the FX an the Time-To-Live of the effect is added to the Ball prefab using a 
- Baker Authoring MonoBehaviour (***ImpactVfxAuthoring***).
- **TimeToLiveComponent**: Controls the time to live of an entity that this component is added to, before it is "destroyed" by the ***TimeToLiveSystem***

#### Systems
- **SpawnSphereSystem**: Spawns a Sphere at the distance over a Ray contained in the **SpawnRequestComponentBuffer**. The Sphere to spawn is one of the prefabs in the **SpheresHolderComponent** singleton.
- **TimeToLiveSystem**: Destroys the entities after the TTL is reached, by looking and updating the TimeToLiveComponent of the entities
```csharp
        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            var ecb = SystemAPI.GetSingleton<EndSimulationEntityCommandBufferSystem.Singleton>().CreateCommandBuffer(state.WorldUnmanaged);
            foreach (var (ttl, entity) in SystemAPI.Query<RefRO<TimeToLiveComponent>>().WithEntityAccess()) {
                if (ttl.ValueRO.CreatedAt + ttl.ValueRO.TimeToLive < SystemAPI.Time.ElapsedTime) {
                    ecb.DestroyEntity(entity);
                }
            }
        }
```

- **ImpactVfxSystem**: Spawns the particle FX at the contact point of the sphere with the box collider, the contact point is calculated by the ***PhysicsWorldSystem***, both the 
sphere and the collider have a PhysicShape configured to "interact" as triggers.
```csharp
        [BurstCompile]
        public void OnUpdate(ref SystemState state) {
            state.CompleteDependency();
            
            var ecb = SystemAPI.GetSingleton<BeginSimulationEntityCommandBufferSystem.Singleton>()
                .CreateCommandBuffer(state.WorldUnmanaged);
            var pws = SystemAPI.GetSingleton<PhysicsWorldSingleton>();
            var simulationSingleton = SystemAPI.GetSingleton<SimulationSingleton>();
            var simulation = simulationSingleton.AsSimulation();

            _vfxLookup.Update(ref state);

            foreach (var triggerEvent in simulation.TriggerEvents) {
                var collisionFilterA = pws.Bodies[triggerEvent.BodyIndexA].Collider.Value.GetCollisionFilter();
                var collisionFilterB = pws.Bodies[triggerEvent.BodyIndexB].Collider.Value.GetCollisionFilter();
                if (!CollisionFilter.IsCollisionEnabled(collisionFilterA, collisionFilterB)) return;

                int bodyIndexSphere;
                int bodyIndexCube;
                CollisionFilter collisionFilter;
                Entity vfxPrefab;
                
                if (_vfxLookup.HasComponent(triggerEvent.EntityA)) {
                    bodyIndexSphere = triggerEvent.BodyIndexA;
                    bodyIndexCube = triggerEvent.BodyIndexB;
                    collisionFilter = collisionFilterA;
                    vfxPrefab = _vfxLookup[triggerEvent.EntityA].VfxPrefab;
                } else {
                    bodyIndexSphere = triggerEvent.BodyIndexB;
                    bodyIndexCube = triggerEvent.BodyIndexA;
                    collisionFilter = collisionFilterB;
                    vfxPrefab = _vfxLookup[triggerEvent.EntityB].VfxPrefab;
                }
                
                PointDistanceInput distanceInput = new PointDistanceInput {
                    Position = pws.Bodies[bodyIndexSphere].WorldFromBody.pos,
                    MaxDistance = 10f,
                    Filter = collisionFilter
                };
                
                if (pws.Bodies[bodyIndexCube].CalculateDistance(distanceInput, out var distanceHit)) {
                    var vfx = ecb.Instantiate(vfxPrefab);
                    ecb.AddComponent(vfx,LocalTransform.FromPosition(distanceHit.Position));
                }
            }
        }
```

**Note**: The System could potentially be improved by scheduling **ITriggerEventsJob**, if considering hundreds of triggers per frame.

#### Others
- **ColliderTestInputManager**: MonoBehaviour that listens to the mouse input and sends the Ray and clicked button (as int) to the ECS world, 
via *SpawnReqestComponentBuffer* for spawning the spheres.

```csharp
        private void OnEnable() {
            mainCamera = mainCamera == null ? Camera.main : mainCamera;
            _world = World.DefaultGameObjectInjectionWorld;

            if (_world.IsCreated) {
                if (!_world.EntityManager.Exists(_spawnRequestBuffer)) {
                    _spawnRequestBuffer = _world.EntityManager.CreateSingletonBuffer<SpawnRequestComponentBuffer>();
                }
            }
        }
```

![TriggerFx.gif](webimg%2FTriggerFx.gif)

---

### 10 Swarm Magnet Scene
([Assets/Scripts/SwarmSpawner](Assets/Scripts/SwarmSpawner) namespace)

The main idea of this was to control the position of an entity using a very simple "Character Controller" on the MonoBehaviour side, using the "New" Input System.

In addition, and to make things more interesting, there are two spawn points from where small balls are spawned and move towards the controlled entity as a "magnet" effect,
or more like a swarm of bees following the queen bee. The bees in this case are moved by "impulse" using physics and given the acceleration towards a random point around the controlled entity,
they usually move past the part point and then "correct" their path to a new random point, giving the swarm feeling.

There is no TTL assigned to the "bees" and they will keep spawning consistently at a rate specified in the **SpawnComponentData** authored in each spawner.

#### Components
- **AreaComponentData**: Holds a float 3 that defines a box area around the controlled entity, used by the *FloatTowardJobSystem* to calculate the random target position.
- **FloatTargetAreaTag**: The controlled entity is marked with this tag component for convenience, to be used by the *FloatTowardJobSystem*.
- **FloatTowardsComponentData**: The component assigned to the "swarm" entities, holds the target position to "fly" to, the move speed (impulse)
and the rate in which the change target position, whether or not the target position is reached.
- **RandomComponent**: This component is assigned when "bees" are spawned by the **SpawnSystem** it contains a burst-compatible `Unity.Mathematics.Random` that is used to calculate the random target position in **FloatTowardJobSystem**.

#### Systems
- **FloatTowardJobSystem**: This system is responsible for the "swarm" movement, it calculates a new random target position for the entities and moves them towards it using physics impulse.
- **SpawnSystem**: Spawns the "swarm" entities at the defined spawn points, and assigns the **RandomComponent** to them.
- **BallCounterSystem**: Counts the number of "swarm" entities and fires an event if the count is different from the previous value.

#### Others
- **MagnetController**: MonoBehaviour that listens to the input from the "New" Input System and syncs the position of the "target" area directly EntityManager.
```csharp
        private void SyncEcsMagnetPosition() {
            //Does this check impacts the performance??
            if (!_world.IsCreated || !_entityManager.Exists(_magnetAreaEntity)) {
                Debug.LogError($"Cannot sync magnet position. World is not created or magnet area entity does not exist.");
                return;
            }
            _entityManager.SetComponentData(_magnetAreaEntity, LocalTransform.FromPosition(transform.position));
        }
```
- **BallCounter**: MonoBehaviour that listens to the event from the **BallCounterSystem** and updates the UI to show the number of "swarm" entities.

![Swarm.gif](webimg%2FSwarm.gif)

---

## Worth Mentioning outside the exercises scenes

Each Scene contains a MonoBehaviour GameObject that "reset" the ECS World when the scene "unloads" and the GameObject is destroyed, this is to avoid "leaking" of the ECS World and its systems,

```csharp
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
```

