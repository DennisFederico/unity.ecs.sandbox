using UnityEngine;
using Utils.Narkdagas.PathFinding.MonoTester;

namespace AStarJobs {
    
    public class UpdateArmyCount : MonoBehaviour {
        [SerializeField] private AnimatedPathfindingJobMonoTester eventHolder;
        [SerializeField] private TMPro.TextMeshProUGUI armyCountText;
        
        private void OnEnable() {
            eventHolder.NewArmySizeEvent += OnNewArmySizeEvent;
        }
        private void OnNewArmySizeEvent(object sender, int count) {
            armyCountText.text = $"<align=center>Total: {count}</align>";
        }

        private void OnDisable() {
            eventHolder.NewArmySizeEvent -= OnNewArmySizeEvent;
        }
    }
}