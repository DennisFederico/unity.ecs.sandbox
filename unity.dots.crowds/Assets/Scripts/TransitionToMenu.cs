using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils.Narkdagas.SceneManagement;

public class TransitionToMenu : MonoBehaviour {
    // Start is called before the first frame update
    IEnumerator Start() {
        SceneManager.LoadScene(SceneLoader.Scenes.MainMenuScene.ToString());
        var endTransition = TransitionManager.Instance.EndTransition();
        yield return endTransition;
    }
}