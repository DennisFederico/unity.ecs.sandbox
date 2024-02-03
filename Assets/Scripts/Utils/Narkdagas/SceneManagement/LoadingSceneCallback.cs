using System.Collections;
using UnityEngine;

namespace Utils.Narkdagas.SceneManagement {
    public class LoadingSceneCallback : MonoBehaviour {

        private IEnumerator Start() {
            SceneLoader.LoaderCallback();
            yield return null;
        }
    }
}
