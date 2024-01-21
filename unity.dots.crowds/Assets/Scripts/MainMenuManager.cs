using CodeMonkey.Utils;
using UnityEngine;
using Utils.Narkdagas.SceneManagement;

public class MainMenuManager : MonoBehaviour {

    [SerializeField] private Button_UI systemsLoaderButton;
    [SerializeField] private Button_UI aStarSceneEcsButton;
    [SerializeField] private Button_UI aStarSceneJobsButton;
    [SerializeField] private Button_UI simpleCrowdsSpawnerButton;
    [SerializeField] private Button_UI ecsSelectionSandboxButton;
    [SerializeField] private Button_UI swarmSpawnerSceneButton;
    [SerializeField] private Button_UI teamSwitchSceneButton;
    [SerializeField] private Button_UI formationChangeSceneButton;
    [SerializeField] private Button_UI towerDefenseSceneButton;
    
    public enum Scenes {
        SystemsLoaderTest,
        AStarPathfindingEcs,
        AStarPathfindingJobs,
        SimpleCrowdsSpawner,
        EcsSelectionSandbox,
        SwarmSpawnerScene,
        TeamSwitchSceneTest,
        FormationChangeScene,
        TowerDefenseScene
    }
    
    private void Awake() {
        systemsLoaderButton.ClickFunc = () => {
            Debug.Log("Loading SystemsLoaderScene");
            SceneLoader.Load(MainMenuManager.Scenes.SystemsLoaderTest);
        };
        aStarSceneEcsButton.ClickFunc = () => {
            Debug.Log("Loading AStarScene ECS");
            SceneLoader.Load(MainMenuManager.Scenes.AStarPathfindingEcs);
        };
        aStarSceneJobsButton.ClickFunc = () => {
            Debug.Log("Loading AStarScene Mono+Jobs");
            SceneLoader.Load(MainMenuManager.Scenes.AStarPathfindingJobs);
        };
        simpleCrowdsSpawnerButton.ClickFunc = () => {
            Debug.Log("Loading SimpleCrowdsSpawnerScene");
            SceneLoader.Load(MainMenuManager.Scenes.SimpleCrowdsSpawner);
        };
        ecsSelectionSandboxButton.ClickFunc = () => {
            Debug.Log("Loading EcsSelectionSandbox");
            SceneLoader.Load(MainMenuManager.Scenes.EcsSelectionSandbox);
        };
        swarmSpawnerSceneButton.ClickFunc = () => {
            Debug.Log("Loading SwarmSpawnerScene");
            SceneLoader.Load(MainMenuManager.Scenes.SwarmSpawnerScene);
        };
        teamSwitchSceneButton.ClickFunc = () => {
            Debug.Log("Loading TeamSwitchSceneTest");
            SceneLoader.Load(MainMenuManager.Scenes.TeamSwitchSceneTest);
        };
        formationChangeSceneButton.ClickFunc = () => {
            Debug.Log("Loading FormationChangeScene");
            SceneLoader.Load(MainMenuManager.Scenes.FormationChangeScene);
        };
        towerDefenseSceneButton.ClickFunc = () => {
            Debug.Log("Loading TowerDefenseScene");
            SceneLoader.Load(MainMenuManager.Scenes.TowerDefenseScene);
        };
    }
}