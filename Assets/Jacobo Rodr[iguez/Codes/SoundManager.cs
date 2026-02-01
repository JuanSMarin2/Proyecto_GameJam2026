using System;
using UnityEngine;
using UnityEngine.Audio;

   
    public class SoundManager : MonoBehaviour
    {
        [SerializeField] private SoundsSO SO;
        private static SoundManager instance = null;
        private AudioSource audioSource;

        private static float globalVolume = 1f;
        private static float normalGlobalVolume = 1f;

        private void Awake()
        {
            if(!instance)
            {
                instance = this;
                audioSource = GetComponent<AudioSource>();
            }
        }

        public static void SetGlobalVolume(float value01)
        {
            globalVolume = Mathf.Clamp01(value01);
            AudioListener.volume = globalVolume;
        }

        public static void LowerGlobalVolume(float value01 = 0.25f)
        {
            normalGlobalVolume = globalVolume;
            SetGlobalVolume(value01);
        }

        public static void RestoreGlobalVolume()
        {
            SetGlobalVolume(normalGlobalVolume);
        }

        public static void PlaySound(SoundType sound, AudioSource source = null, float volume = 1)
        {
            if (instance == null)
            {
                Debug.LogWarning("[SoundManager] No instance in scene. Cannot play sound: " + sound);
                return;
            }

            SoundList soundList = instance.SO.sounds[(int)sound];
            AudioClip[] clips = soundList.sounds;
            AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];

            if(source)
            {
                source.outputAudioMixerGroup = soundList.mixer;
                source.clip = randomClip;
                source.volume = volume * soundList.volume * globalVolume;
                source.Play();
            }
            else
            {
                instance.audioSource.outputAudioMixerGroup = soundList.mixer;
                instance.audioSource.PlayOneShot(randomClip, volume * soundList.volume * globalVolume);
            }
        }
    }

    [Serializable]
    public struct SoundList
    {
        [HideInInspector] public string name;
        [Range(0, 1)] public float volume;
        public AudioMixerGroup mixer;
        public AudioClip[] sounds;
    }