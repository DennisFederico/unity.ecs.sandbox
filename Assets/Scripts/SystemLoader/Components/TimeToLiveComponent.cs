using Unity.Entities;

namespace SystemLoader.Components {
    public struct TimeToLiveComponent : IComponentData {
        public float TimeToLive;
		public double BirthTime;
    }
}