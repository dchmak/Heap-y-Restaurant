/*
* Created by Daniel Mak
*/

using UnityEngine;

namespace Daniel {
	public class PlayAudio : MonoBehaviour {
        AudioManager audioManager;

        private void Start() {
            audioManager = AudioManager.instance;
        }

        public void Play(string audioName) {
            if (!audioManager.IsPlaying(audioName)) {
                audioManager.Play(audioName);
            }
        }
	}
}