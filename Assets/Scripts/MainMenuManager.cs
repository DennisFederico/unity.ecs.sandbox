using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Utils.Narkdagas.SceneManagement;

public class MainMenuManager : MonoBehaviour {

    [SerializeField] private Button systemsLoaderButton;
    [SerializeField] private Button aStarSceneEcsButton;
    [SerializeField] private Button aStarSceneJobsButton;
    [SerializeField] private Button simpleCrowdsSpawnerButton;
    [SerializeField] private Button ecsSelectionSandboxButton;
    [SerializeField] private Button swarmSpawnerSceneButton;
    [SerializeField] private Button colliderTestSceneButton;
    [SerializeField] private Button teamSwitchSceneButton;
    [SerializeField] private Button formationChangeSceneButton;
    [SerializeField] private Button towerDefenseSceneHybridButton;
    [SerializeField] private Button towerDefenseSceneEcsButton;
    [SerializeField] private Button exitButton;
    
    public enum Scenes {
        SystemsLoaderTest,
        AStarPathfindingEcs,
        AStarPathfindingJobs,
        SimpleCrowdsSpawner,
        EcsSelectionSandbox,
        SwarmSpawnerScene,
        ColliderTest, 
        TeamSwitchSceneTest,
        FormationChangeScene,
        TowerDefenseGrids,
        TowerDefenseScene
    }

    private void Awake() {
        systemsLoaderButton.onClick.AddListener(() => {
            Debug.Log("Loading SystemsLoaderScene");
            SceneLoader.Load(Scenes.SystemsLoaderTest, true);
        });
        aStarSceneEcsButton.onClick.AddListener(() => {
            Debug.Log("Loading AStarScene ECS");
            SceneLoader.LoadAsync(Scenes.AStarPathfindingEcs, true);
        });
        aStarSceneJobsButton.onClick.AddListener(() => {
            Debug.Log("Loading AStarScene Mono+Jobs");
            SceneLoader.LoadAsync(Scenes.AStarPathfindingJobs, true);
        });
        simpleCrowdsSpawnerButton.onClick.AddListener(() => {
            Debug.Log("Loading SimpleCrowdsSpawnerScene");
            SceneLoader.Load(Scenes.SimpleCrowdsSpawner, true);
        });
        ecsSelectionSandboxButton.onClick.AddListener(() => {
            Debug.Log("Loading EcsSelectionSandbox");
            SceneLoader.Load(Scenes.EcsSelectionSandbox, true);
        });
        swarmSpawnerSceneButton.onClick.AddListener(() => {
            Debug.Log("Loading SwarmSpawnerScene");
            SceneLoader.Load(Scenes.SwarmSpawnerScene, true);
        });
        colliderTestSceneButton.onClick.AddListener(() => {
            Debug.Log("Loading ColliderTest");
            SceneLoader.Load(Scenes.ColliderTest, true);
        });
        teamSwitchSceneButton.onClick.AddListener(() => {
            Debug.Log("Loading TeamSwitchSceneTest");
            SceneLoader.Load(Scenes.TeamSwitchSceneTest, true);
        });
        formationChangeSceneButton.onClick.AddListener(() => {
            Debug.Log("Loading FormationChangeScene");
            SceneLoader.Load(Scenes.FormationChangeScene, true);
        });
        towerDefenseSceneHybridButton.onClick.AddListener(() => {
            Debug.Log("Loading TowerDefenseGridsScene");
            SceneLoader.Load(Scenes.TowerDefenseGrids, true);
        });
        towerDefenseSceneEcsButton.onClick.AddListener(() => {
            Debug.Log("Loading TowerDefenseScene");
            SceneLoader.Load(Scenes.TowerDefenseScene, true);
        });
        exitButton.onClick.AddListener(Application.Quit);
    }
}