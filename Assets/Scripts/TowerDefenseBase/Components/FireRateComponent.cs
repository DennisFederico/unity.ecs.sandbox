using Unity.Entities;

namespace TowerDefenseBase.Components {
    public struct FireRateComponent : IComponentData {

        public float MaxTimer;
        public float CurrentTimer;

        public void StartOverTimer() {
            CurrentTimer = MaxTimer;
        }
        
        public void ResetTimer() {
            CurrentTimer += MaxTimer;
        }
        
        public void SetTimer(float value) {
            CurrentTimer = value;
        }

        public float ElapseTime(float value) {
            CurrentTimer -= value;
            return CurrentTimer;
        }

        public float RemainingTime() {
            return CurrentTimer;
        }

        public readonly bool HasElapsed() {
            return CurrentTimer < 0f;
        }
    }
}