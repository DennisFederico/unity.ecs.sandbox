using UnityEngine;
using UnityEngine.UI;
using Utils.Narkdagas.SceneManagement;

namespace Utils {
    [RequireComponent(typeof (Button))]
    public class MainMenuButtonAction : MonoBehaviour{
        
        private void Awake() {
            var mainMenuButton = GetComponent<Button>();
            mainMenuButton.onClick.AddListener(GoToMenu);
        }
        
        private void GoToMenu() {
            SceneLoader.Load(SceneLoader.Scenes.MainMenuScene, true);
        }

        private void OnDisable() {
            var mainMenuButton = GetComponent<Button>();
            mainMenuButton.onClick.RemoveListener(GoToMenu);
        }
    }
}