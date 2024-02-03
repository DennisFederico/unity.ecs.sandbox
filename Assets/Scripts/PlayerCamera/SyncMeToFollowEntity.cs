using System.Collections.Generic;
using UnityEngine;

namespace PlayerCamera {
    public class SyncMeToFollowEntity : MonoBehaviour {
        public string myId;
        public bool position = true;
        public bool rotation = true;
        
        //This is a static list that each GameObject components
        //Registers to during Awake for sync to an entity
        //To improve performance, this could be a HashMap instead of a List
        public static readonly List<SyncMeToFollowEntity> Followers = new();

        private void Awake() => Followers.Add(this);

        private void OnDisable() => Followers.Remove(this);
    }
}