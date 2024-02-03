using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utils.Narkdagas.SceneManagement;

public class TransitionToMenu : MonoBehaviour {

    private IEnumerator Start() {
        SceneManager.LoadScene(SceneLoader.Scenes.MainMenuScene.ToString());
        var endTransition = TransitionManager.Instance.EndTransition();
        yield return endTransition;
    }
}