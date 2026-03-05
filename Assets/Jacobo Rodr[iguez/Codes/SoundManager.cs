using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

   
    public class SoundManager : MonoBehaviour
    {
        [SerializeField] private SoundsSO SO;
        private static SoundManager instance = null;
        private AudioSource audioSource;

        [Header("SFX Pool")]
        [SerializeField] private int initialPoolSize = 10;
        [SerializeField] private bool expandPoolIfNeeded = true;
        [SerializeField] private int maxPoolSize = 32;
        [SerializeField] private bool allowVoiceStealing = true;

        private readonly List<AudioSource> sfxPool = new List<AudioSource>();
        private int stealIndex;

        private static float globalVolume = 1f;
        private static float normalGlobalVolume = 1f;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

            WarmPool();
        }

        private void WarmPool()
        {
            sfxPool.Clear();

            int size = Mathf.Max(1, initialPoolSize);
            for (int i = 0; i < size; i++)
            {
                sfxPool.Add(CreatePooledSource(i));
            }
        }

        private AudioSource CreatePooledSource(int index)
        {
            GameObject go = new GameObject($"SFX_{index:00}");
            go.transform.SetParent(transform, false);

            AudioSource src = go.AddComponent<AudioSource>();
            CopyAudioSourceSettings(audioSource, src);
            return src;
        }

        private static void CopyAudioSourceSettings(AudioSource template, AudioSource target)
        {
            if (target == null) return;

            target.playOnAwake = false;
            target.loop = false;

            if (template == null) return;

            target.mute = template.mute;
            target.bypassEffects = template.bypassEffects;
            target.bypassListenerEffects = template.bypassListenerEffects;
            target.bypassReverbZones = template.bypassReverbZones;
            target.priority = template.priority;
            target.pitch = 1f;
            target.panStereo = template.panStereo;
            target.spatialBlend = template.spatialBlend;
            target.reverbZoneMix = template.reverbZoneMix;
            target.dopplerLevel = template.dopplerLevel;
            target.spread = template.spread;
            target.rolloffMode = template.rolloffMode;
            target.minDistance = template.minDistance;
            target.maxDistance = template.maxDistance;
        }

        private AudioSource GetSfxSource()
        {
            for (int i = 0; i < sfxPool.Count; i++)
            {
                if (!sfxPool[i].isPlaying)
                    return sfxPool[i];
            }

            if (expandPoolIfNeeded && sfxPool.Count < Mathf.Max(1, maxPoolSize))
            {
                AudioSource src = CreatePooledSource(sfxPool.Count);
                sfxPool.Add(src);
                return src;
            }

            if (!allowVoiceStealing || sfxPool.Count == 0)
                return null;

            stealIndex = (stealIndex + 1) % sfxPool.Count;
            AudioSource stolen = sfxPool[stealIndex];
            stolen.Stop();
            return stolen;
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
                source.volume = volume * soundList.volume * globalVolume;

                // Permite solapar sonidos en el mismo AudioSource.
                source.PlayOneShot(randomClip);
            }
            else
            {
                AudioSource sfx = instance.GetSfxSource();
                if (sfx == null) return;

                sfx.outputAudioMixerGroup = soundList.mixer;
                sfx.volume = volume * soundList.volume * globalVolume;
                sfx.clip = randomClip;
                sfx.Play();
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