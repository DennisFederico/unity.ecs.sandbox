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

### 1. Bootstrap & MainMenu

The bootstrap it's just a "launcher" to load the scene loader and scene transitions manager, the main menu scene is loaded from here and it's a simple scene with a few buttons to load each test/demo scene in the Sandbox.

### 2. Formation Change Demo

Here we have 2 "Towers" (Entities) that controls the number of units on their armies and the formation they should follow around them.

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
- **ChangeFormationSystem**, this system *Update* method runs every 3 seconds aprox. It Queries for TowerComponent with **no** SpawnUnitsTag (disabled), meaning that their units have already spawned and changes the formation value of the Tower (not the units).
- **UnitChangeFormationSystem** queries for all *FormationComponent* and the *ParentEntityReferenceComponent*, if the unit and the tower formation are different, ***a new position is calculated based on the formation dictated by the tower***. 

#### Others
- *PositionUtils* class contain helper methods to calculate the position of the units based on the formation and the tower position.

![Formations.gif](webimg%2FFormations.gif)

---

### 3. Team/Color Switch Demo
--TODO Description

![TeamSwitch.gif](webimg%2FTeamSwitch.gif)

### 4. Pathfinding (MonoBehaviour) Demo
--TODO Description

![PathJobs.gif](webimg%2FPathJobs.gif)

### 5. Pathfinding (ECS) Demo
--TODO Description

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




