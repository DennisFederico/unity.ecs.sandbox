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
--TODO Description

![Formations.gif](webimg%2FFormations.gif)

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




