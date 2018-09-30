/*
 * Created by Daniel Mak
 */

using System;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour {

    #region Sound Class
    [System.Serializable]
    public class Sound {
        public string name;

        public AudioClip clip;
        public AudioMixerGroup mixer;

        [Range(0f, 1f)]
        public float volume = 0.5f;

        [Range(0.1f, 3f)]
        public float pitch = 1f;

        [HideInInspector]
        public AudioSource source;

        public bool loop;
    }
    #endregion

    public bool autoPlayFirst = false;

    [Space]

    public Sound[] sounds;

    #region Singleton
    public static AudioManager instance;

	private void Awake () {
        if (instance == null) instance = this;
        else {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
	}
    #endregion

    public void Play(string name) {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null) {
            Debug.LogWarning("Sound " + name + " does not exist.");
            return;
        }

        s.source.Play();
    }

    public void Stop() {
        foreach (Sound sound in sounds) {
            sound.source.Stop();
        }
    }

    public void Stop(string name) {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null) {
            Debug.LogWarning("Sound " + name + " does not exist.");
        }
        s.source.Stop();
    }

    public bool IsPlaying(string name) {
        Sound s = Array.Find(sounds, sound => sound.name == name);
        if (s == null) {
            Debug.LogWarning("Sound " + name + " does not exist.");
            return false;
        }
        return s.source.isPlaying;
    }

    public AudioSource CurrentlyPlaying() {
        foreach (Sound sound in sounds) {
            if (sound.source.isPlaying) return sound.source;
        }
        return null;
    }

    private void Start() {
        foreach (Sound sound in sounds) {
            sound.source = gameObject.AddComponent<AudioSource>();

            sound.source.clip = sound.clip;
            sound.source.outputAudioMixerGroup = sound.mixer;
            sound.source.volume = sound.volume;
            sound.source.pitch = sound.pitch;
            sound.source.loop = sound.loop;
        }

        if (autoPlayFirst) Play(sounds[0].name);
    }

    private void OnValidate() {
        name = "[Audio Manager]";
    }
}
