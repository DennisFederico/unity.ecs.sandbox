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

        public static void Load(Enum scene) {
            // Set the loader callback action to load the target scene
            _onLoaderCallback = () => {
                GameObject loadingGameObject = new GameObject("Loading Game Object");
                loadingGameObject.AddComponent<LoadingMonoBehaviour>().StartCoroutine(LoadSceneAsync(scene));
            };

            // Load the loading scene
            SceneManager.LoadScene(Scenes.LoadingScene.ToString());
        }

        private static IEnumerator LoadSceneAsync(Enum scene) {
            yield return null;
            _loadingAsyncOperation = SceneManager.LoadSceneAsync(scene.ToString());
            yield return null;
            while (!_loadingAsyncOperation.isDone) {
                yield return null;
            }
        }

        public static float GetLoadingProgress() {
            if (_loadingAsyncOperation != null) {
                return _loadingAsyncOperation.progress;
            } else {
                return 1f;
            }
        }

        public static void LoaderCallback() {
            // Triggered after the first Update which lets the screen refresh
            // Execute the loader callback action which will load the target scene
            if (_onLoaderCallback != null) {
                _onLoaderCallback();
                _onLoaderCallback = null;
            }
        }
    }
}