using System.Collections;
using UnityEngine;

namespace Utils.Narkdagas.SceneManagement {
    public class TransitionManager : MonoBehaviour {
        public static TransitionManager Instance { get; private set;}
        [SerializeField] private Animator transitionAnimator;
        [SerializeField] private Canvas transitionCanvas;
        private static readonly int StartFade = Animator.StringToHash("StartFade");
        private static readonly int EndFade = Animator.StringToHash("EndFade");
        
        private void Awake() {
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                throw new System.Exception("An instance of this singleton already exists.");
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public Coroutine StartTransition() {
            return StartCoroutine(StartTransitionCoroutine());
        }
        
        IEnumerator StartTransitionCoroutine() {
            transitionCanvas.gameObject.SetActive(true);
            transitionAnimator.SetTrigger(StartFade);
            yield return new WaitForSeconds(.5f);
        }
        
        public Coroutine EndTransition() {
            return StartCoroutine(EndTransitionCoroutine());
        }
        
        IEnumerator EndTransitionCoroutine() {
            transitionCanvas.gameObject.SetActive(true);
            transitionAnimator.SetTrigger(EndFade);
            yield return new WaitForSeconds(.5f);
            transitionCanvas.gameObject.SetActive(false);
        }
    }
}