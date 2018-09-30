/*
 * Created by Daniel Mak
 */

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class MasterVolumeSliderFunctions : MonoBehaviour {

    public AudioMixer audioMixer;

    private void Awake() {
        float val;
        audioMixer.GetFloat("MasterVolume", out val);
        GetComponent<Slider>().value = val;
    }

	public void SetVolume(float volume) {
        audioMixer.SetFloat("MasterVolume", volume);
    }
}
