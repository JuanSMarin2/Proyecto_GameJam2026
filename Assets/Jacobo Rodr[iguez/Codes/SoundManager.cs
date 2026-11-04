using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

   
    public class SoundManager : MonoBehaviour
    {
        [SerializeField] private SoundsSO SO;
        private static SoundManager instance = null;
        private AudioSource audioSource;

        private const string PrefSfxVolume = "audio.sfx.volume";
        private const string PrefMusicVolume = "audio.music.volume";
        private const string PrefSfxLastNonZero = "audio.sfx.lastNonZero";
        private const string PrefMusicLastNonZero = "audio.music.lastNonZero";

        private static bool prefsLoaded;
        private static float sfxUserVolume = 1f;
        private static float musicUserVolume = 1f;
        private static float sfxLastNonZeroVolume = 1f;
        private static float musicLastNonZeroVolume = 1f;
        private static float pauseDucking = 1f;
        private static float normalPauseDucking = 1f;
        private static float collectionMusicDucking = 1f;

        [Header("SFX Pool")]
        [SerializeField] private int initialPoolSize = 10;
        [SerializeField] private bool expandPoolIfNeeded = true;
        [SerializeField] private int maxPoolSize = 32;
        [SerializeField] private bool allowVoiceStealing = true;

        [Header("Collection Music Ducking")]
        [SerializeField, Range(0f, 1f)] private float collectionMusicVolumeMultiplier = 0.2f;
        [SerializeField, Min(0f)] private float collectionMusicFadeOutSeconds = 0.1f;
        [SerializeField, Min(0f)] private float collectionMusicFadeInSeconds = 0.15f;

        [Header("Debug")]
        [SerializeField] private bool debugSceneAudioSync = false;

        private readonly List<AudioSource> sfxPool = new List<AudioSource>();
        private int stealIndex;
        private SceneAudioSourceVolumeSync sceneAudioSourceVolumeSync;
        private Coroutine collectionMusicDuckingRoutine;

        private void Awake()
        {
            instance = this;
            collectionMusicDucking = 1f;
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null) audioSource = gameObject.AddComponent<AudioSource>();

            EnsurePrefsLoaded();

            RemoveLegacyPoolChildren();
            WarmPool();
            sceneAudioSourceVolumeSync = new SceneAudioSourceVolumeSync(
                SO,
                transform,
                audioSource,
                GetEffectiveSfxVolume,
                debugSceneAudioSync
            );
              sceneAudioSourceVolumeSync?.ApplyConfiguredVolumesToSceneAudioSources(SceneManager.GetActiveScene());
              if(sceneAudioSourceVolumeSync == null)
              {
                  Debug.LogWarning("SceneAudioSourceVolumeSync is null. Scene audio source volumes will not be synced.");
              }
              else if (debugSceneAudioSync)
              {
                  Debug.Log("SceneAudioSourceVolumeSync initialized successfully.");
              }
             
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }


        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;

            if (instance == this)
            {
                collectionMusicDucking = 1f;
                instance = null;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            sceneAudioSourceVolumeSync?.ApplyConfiguredVolumesToSceneAudioSources(scene);
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

        private void RemoveLegacyPoolChildren()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);
                if (!child.name.StartsWith("SFX_") || child.GetComponent<AudioSource>() == null)
                    continue;

                Destroy(child.gameObject);
            }
        }

        private AudioSource CreatePooledSource(int index)
        {
            // El pool conserva fuentes independientes para solapar efectos, pero no crea
            // GameObjects hijos visibles en la jerarquía.
            AudioSource src = gameObject.AddComponent<AudioSource>();
            CopyAudioSourceSettings(audioSource, src);
            src.hideFlags = HideFlags.HideInInspector;
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

        private static void EnsurePrefsLoaded()
        {
            if (prefsLoaded)
                return;

            sfxUserVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(PrefSfxVolume, 1f));
            musicUserVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(PrefMusicVolume, 1f));

            sfxLastNonZeroVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(PrefSfxLastNonZero, Mathf.Max(0.01f, sfxUserVolume)));
            musicLastNonZeroVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(PrefMusicLastNonZero, Mathf.Max(0.01f, musicUserVolume)));

            if (sfxLastNonZeroVolume <= 0f) sfxLastNonZeroVolume = 1f;
            if (musicLastNonZeroVolume <= 0f) musicLastNonZeroVolume = 1f;

            prefsLoaded = true;
        }

        private static void SavePrefs()
        {
            PlayerPrefs.SetFloat(PrefSfxVolume, sfxUserVolume);
            PlayerPrefs.SetFloat(PrefMusicVolume, musicUserVolume);
            PlayerPrefs.SetFloat(PrefSfxLastNonZero, sfxLastNonZeroVolume);
            PlayerPrefs.SetFloat(PrefMusicLastNonZero, musicLastNonZeroVolume);
            PlayerPrefs.Save();
        }

        public static float GetSfxVolume()
        {
            EnsurePrefsLoaded();
            return sfxUserVolume;
        }

        public static float GetMusicVolume()
        {
            EnsurePrefsLoaded();
            return musicUserVolume;
        }

        public static float GetEffectiveSfxVolume()
        {
            EnsurePrefsLoaded();
            return sfxUserVolume * pauseDucking;
        }

        public static float GetEffectiveMusicVolume()
        {
            EnsurePrefsLoaded();
            return musicUserVolume * pauseDucking * collectionMusicDucking;
        }

        public static void SetSfxVolume(float value01)
        {
            EnsurePrefsLoaded();

            sfxUserVolume = Mathf.Clamp01(value01);
            if (sfxUserVolume > 0.0001f)
                sfxLastNonZeroVolume = sfxUserVolume;

            SavePrefs();
            RefreshSyncedSceneSfxAudioSources();
        }

        public static void SetMusicVolume(float value01)
        {
            EnsurePrefsLoaded();

            musicUserVolume = Mathf.Clamp01(value01);
            if (musicUserVolume > 0.0001f)
                musicLastNonZeroVolume = musicUserVolume;

            SavePrefs();
        }

        public static void MuteSfx()
        {
            EnsurePrefsLoaded();

            if (sfxUserVolume > 0.0001f)
                sfxLastNonZeroVolume = sfxUserVolume;

            sfxUserVolume = 0f;
            SavePrefs();
            RefreshSyncedSceneSfxAudioSources();
        }

        public static void UnmuteSfxRestore()
        {
            EnsurePrefsLoaded();

            if (sfxUserVolume > 0.0001f)
                return;

            sfxUserVolume = Mathf.Clamp01(Mathf.Max(0.01f, sfxLastNonZeroVolume));
            SavePrefs();
            RefreshSyncedSceneSfxAudioSources();
        }

        public static void MuteMusic()
        {
            EnsurePrefsLoaded();

            if (musicUserVolume > 0.0001f)
                musicLastNonZeroVolume = musicUserVolume;

            musicUserVolume = 0f;
            SavePrefs();
        }

        public static void UnmuteMusicRestore()
        {
            EnsurePrefsLoaded();

            if (musicUserVolume > 0.0001f)
                return;

            musicUserVolume = Mathf.Clamp01(Mathf.Max(0.01f, musicLastNonZeroVolume));
            SavePrefs();
        }

        public static void SetGlobalVolume(float value01)
        {
            pauseDucking = Mathf.Clamp01(value01);
            RefreshSyncedSceneSfxAudioSources();
        }

        public static void LowerGlobalVolume(float value01 = 0.25f)
        {
            normalPauseDucking = pauseDucking;
            SetGlobalVolume(value01);
        }

        public static void RestoreGlobalVolume()
        {
            SetGlobalVolume(normalPauseDucking);
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
                source.volume = volume * soundList.volume * GetEffectiveSfxVolume();

                // Permite solapar sonidos en el mismo AudioSource.
                source.PlayOneShot(randomClip);
            }
            else
            {
                AudioSource sfx = instance.GetSfxSource();
                if (sfx == null) return;

                sfx.outputAudioMixerGroup = soundList.mixer;
                sfx.volume = volume * soundList.volume * GetEffectiveSfxVolume();
                sfx.clip = randomClip;
                sfx.Play();
            }

            if (IsCollectionSound(sound))
                instance.StartCollectionMusicDucking(randomClip.length);
        }

        private static bool IsCollectionSound(SoundType sound)
        {
            return sound == SoundType.RecoleccionAfricana
                || sound == SoundType.RecoleccionEuropea
                || sound == SoundType.RecoleccionJaponesa
                || sound == SoundType.RecoleccionLatina;
        }

        private void StartCollectionMusicDucking(float clipDuration)
        {
            if (collectionMusicDuckingRoutine != null)
                StopCoroutine(collectionMusicDuckingRoutine);

            collectionMusicDuckingRoutine = StartCoroutine(CollectionMusicDuckingRoutine(clipDuration));
        }

        private IEnumerator CollectionMusicDuckingRoutine(float clipDuration)
        {
            float duration = Mathf.Max(0f, clipDuration);
            float fadeOut = Mathf.Max(0f, collectionMusicFadeOutSeconds);
            float fadeIn = Mathf.Max(0f, collectionMusicFadeInSeconds);
            float combinedFadeDuration = fadeOut + fadeIn;

            // Si el clip es muy corto, los fades se ajustan para no prolongar el ducking
            // más allá de la duración del sonido de recolección.
            if (duration < combinedFadeDuration && combinedFadeDuration > 0f)
            {
                float scale = duration / combinedFadeDuration;
                fadeOut *= scale;
                fadeIn *= scale;
            }

            yield return FadeCollectionMusicDucking(Mathf.Clamp01(collectionMusicVolumeMultiplier), fadeOut);

            float holdDuration = Mathf.Max(0f, duration - fadeOut - fadeIn);
            if (holdDuration > 0f)
                yield return new WaitForSecondsRealtime(holdDuration);

            yield return FadeCollectionMusicDucking(1f, fadeIn);
            collectionMusicDuckingRoutine = null;
        }

        private IEnumerator FadeCollectionMusicDucking(float target, float duration)
        {
            float start = collectionMusicDucking;
            if (duration <= 0f)
            {
                collectionMusicDucking = target;
                yield break;
            }

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.unscaledDeltaTime;
                collectionMusicDucking = Mathf.Lerp(start, target, Mathf.Clamp01(elapsed / duration));
                yield return null;
            }

            collectionMusicDucking = target;
        }

        private static void RefreshSyncedSceneSfxAudioSources()
        {
            if (instance == null || instance.sceneAudioSourceVolumeSync == null)
                return;

            instance.sceneAudioSourceVolumeSync.ApplyConfiguredVolumesToSceneAudioSources(SceneManager.GetActiveScene());
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
