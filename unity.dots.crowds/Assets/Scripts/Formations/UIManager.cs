using CodeMonkey.Utils;
using UnityEngine;
using Utils.Narkdagas.SceneManagement;

namespace Formations {
    public class UIManager : MonoBehaviour {
        [SerializeField] private Button_UI mainMenuButton;
        
        private void Awake() {
            mainMenuButton.ClickFunc = () => {
                Debug.Log("Loading MainMenuScene");
                SceneLoader.Load(SceneLoader.Scenes.MainMenuScene);
            };
        }
    }
}