using UnityEngine;
using UnityEngine.UI;
using Utils.Narkdagas.SceneManagement;

public class MainMenuManager : MonoBehaviour {

    [SerializeField] private Button systemsLoaderButton;
    [SerializeField] private Button aStarSceneEcsButton;
    [SerializeField] private Button aStarSceneJobsButton;
    [SerializeField] private Button simpleCrowdsSpawnerButton;
    [SerializeField] private Button ecsSelectionSandboxButton;
    [SerializeField] private Button swarmSpawnerSceneButton;
    [SerializeField] private Button teamSwitchSceneButton;
    [SerializeField] private Button formationChangeSceneButton;
    [SerializeField] private Button towerDefenseSceneButton;

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
        systemsLoaderButton.onClick.AddListener(() => {
            Debug.Log("Loading SystemsLoaderScene");
            SceneLoader.Load(Scenes.SystemsLoaderTest);
        });
        aStarSceneEcsButton.onClick.AddListener(() => {
            Debug.Log("Loading AStarScene ECS");
            SceneLoader.Load(Scenes.AStarPathfindingEcs);
        });
        aStarSceneJobsButton.onClick.AddListener(() => {
            Debug.Log("Loading AStarScene Mono+Jobs");
            SceneLoader.Load(Scenes.AStarPathfindingJobs);
        });
        simpleCrowdsSpawnerButton.onClick.AddListener(() => {
            Debug.Log("Loading SimpleCrowdsSpawnerScene");
            SceneLoader.Load(Scenes.SimpleCrowdsSpawner);
        });
        ecsSelectionSandboxButton.onClick.AddListener(() => {
            Debug.Log("Loading EcsSelectionSandbox");
            SceneLoader.Load(Scenes.EcsSelectionSandbox);
        });
        swarmSpawnerSceneButton.onClick.AddListener(() => {
            Debug.Log("Loading SwarmSpawnerScene");
            SceneLoader.Load(Scenes.SwarmSpawnerScene);
        });
        teamSwitchSceneButton.onClick.AddListener(() => {
            Debug.Log("Loading TeamSwitchSceneTest");
            SceneLoader.Load(Scenes.TeamSwitchSceneTest);
        });
        formationChangeSceneButton.onClick.AddListener(() => {
            Debug.Log("Loading FormationChangeScene");
            SceneLoader.Load(Scenes.FormationChangeScene);
        });
        towerDefenseSceneButton.onClick.AddListener(() => {
            Debug.Log("Loading TowerDefenseScene");
            SceneLoader.Load(Scenes.TowerDefenseScene);
        });
    }
}