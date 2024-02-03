using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Utils.Narkdagas.SceneManagement {

    public static class SceneLoader {

        private class LoadingMonoBehaviour : MonoBehaviour {
        }

        private static Action _onLoaderCallback;
        private static AsyncOperation _loadingAsyncOperation;

        public enum Scenes {
            MainMenuScene,
            LoadingScene,
        }

        public static void Load(Enum scene, bool transition = false) {
            if (transition) {
                TransitionManager.Instance.StartCoroutine(LoadScene(scene, true));
            } else {
                SceneManager.LoadScene(scene.ToString());
            }
        }

        public static void LoadAsync(Enum scene, bool transition = false) {
            _onLoaderCallback = () => {
                MonoBehaviour transitionObject = transition ? TransitionManager.Instance : new GameObject("Loader..").AddComponent<LoadingMonoBehaviour>();
                transitionObject.StartCoroutine(LoadSceneAsync(scene, transition));
            };
            MonoBehaviour transitionObject = transition ? TransitionManager.Instance : new GameObject("Loader...").AddComponent<LoadingMonoBehaviour>();
            transitionObject.StartCoroutine(LoadScene(Scenes.LoadingScene, transition));
        }
        
        private static IEnumerator LoadScene(Enum scene, bool transition = false) {
            if (transition) {
                var startTransition = TransitionManager.Instance.StartTransition();
                yield return startTransition;
            }

            SceneManager.LoadScene(scene.ToString());
            if (transition) {
                var endTransition = TransitionManager.Instance.EndTransition();
                yield return endTransition;
            }
        }
        
        private static IEnumerator LoadSceneAsync(Enum scene, bool transition = false) {
            _loadingAsyncOperation = SceneManager.LoadSceneAsync(scene.ToString());
            _loadingAsyncOperation.allowSceneActivation = false;
            while (!_loadingAsyncOperation.isDone) {
                if (_loadingAsyncOperation.progress >= 0.9f) {
                    _loadingAsyncOperation.allowSceneActivation = true;
                    if (transition) {
                        var endTransition = TransitionManager.Instance.EndTransition();
                        yield return endTransition;
                    }
                }
                yield return null;
            }
        }

        public static float GetLoadingProgress() {
            if (_loadingAsyncOperation != null) {
                return Mathf.Clamp01(_loadingAsyncOperation.progress / 0.9f);
            }

            return 1f;
        }

        public static void LoaderCallback() {
            if (_onLoaderCallback == null) return;
            _onLoaderCallback();
            _onLoaderCallback = null;
        }
    }
}