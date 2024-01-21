using CodeMonkey.Utils;
using UnityEngine;
using Utils.Narkdagas.SceneManagement;

namespace SimpleCrowdsSpawn {
    public class ActionMenuManager : MonoBehaviour {
        [SerializeField] private Button_UI mainMenuButton;
        
        private void Awake() {
            mainMenuButton.ClickFunc = () => {
                SceneLoader.Load(SceneLoader.Scenes.MainMenuScene);
            };
        }
    }
}