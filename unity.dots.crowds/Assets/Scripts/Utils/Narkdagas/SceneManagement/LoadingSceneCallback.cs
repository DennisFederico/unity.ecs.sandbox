using System.Collections;
using UnityEngine;

namespace Utils.Narkdagas.SceneManagement {
    public class LoadingSceneCallback : MonoBehaviour {

        private IEnumerator Start() {
            yield return null;
            SceneLoader.LoaderCallback();
        }
    }
}
