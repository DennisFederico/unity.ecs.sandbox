using CodeMonkey.Utils;
using UnityEngine;
using Utils.Narkdagas.SceneManagement;

namespace AStar {
    public class ActionMenuManager : MonoBehaviour {

        [SerializeField] private Button_UI mainMenuButton;
        
        private void Awake() {
            mainMenuButton.ClickFunc = () => {
                Debug.Log("Loading MainMenuScene");
                SceneLoader.Load(SceneLoader.Scenes.MainMenuScene);
            };
        }
    }
}