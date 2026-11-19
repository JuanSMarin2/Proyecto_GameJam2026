using System;
using System.Collections.Generic;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance { get; private set; }

    [Header("FMOD Bus Paths")]
    [SerializeField] private string masterBusPath = "bus:/";
    [SerializeField] private string musicBusPath = "bus:/Music";
    [SerializeField] private string sfxBusPath = "bus:/SFX";

    private const string PrefMasterVolume = "audio.master.volume";
    private const string PrefSfxVolume = "audio.sfx.volume";
    private const string PrefMusicVolume = "audio.music.volume";
    private const string PrefMasterLastNonZero = "audio.master.lastNonZero";
    private const string PrefSfxLastNonZero = "audio.sfx.lastNonZero";
    private const string PrefMusicLastNonZero = "audio.music.lastNonZero";

    private static bool prefsLoaded;
    private static float masterUserVolume = 1f;
    private static float sfxUserVolume = 1f;
    private static float musicUserVolume = 1f;
    private static float masterLastNonZeroVolume = 1f;
    private static float sfxLastNonZeroVolume = 1f;
    private static float musicLastNonZeroVolume = 1f;
    private static float pauseDucking = 1f;
    private static float normalPauseDucking = 1f;

    private Bus masterBus;
    private Bus musicBus;
    private Bus sfxBus;
    private bool hasMasterBus;
    private bool hasMusicBus;
    private bool hasSfxBus;

    private readonly List<EventInstance> activeEvents = new List<EventInstance>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            EnsurePrefsLoaded();
        }
        else
        {
            Debug.Log("Mas de un AudioManager en escena, destruyendo el más nuevo.");
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        InitBuses();
        ApplyAllVolumes();
    }

    #region Bus & Volume Management

    private void InitBuses()
    {
        try
        {
            masterBus = RuntimeManager.GetBus(masterBusPath);
            hasMasterBus = masterBus.isValid();
        }
        catch (Exception)
        {
            hasMasterBus = false;
        }

        try
        {
            musicBus = RuntimeManager.GetBus(musicBusPath);
            hasMusicBus = musicBus.isValid();
        }
        catch (Exception)
        {
            hasMusicBus = false;
        }

        try
        {
            sfxBus = RuntimeManager.GetBus(sfxBusPath);
            hasSfxBus = sfxBus.isValid();
        }
        catch (Exception)
        {
            hasSfxBus = false;
        }
    }

    private static void EnsurePrefsLoaded()
    {
        if (prefsLoaded)
            return;

        masterUserVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(PrefMasterVolume, 1f));
        sfxUserVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(PrefSfxVolume, 1f));
        musicUserVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(PrefMusicVolume, 1f));

        masterLastNonZeroVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(PrefMasterLastNonZero, Mathf.Max(0.01f, masterUserVolume)));
        sfxLastNonZeroVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(PrefSfxLastNonZero, Mathf.Max(0.01f, sfxUserVolume)));
        musicLastNonZeroVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(PrefMusicLastNonZero, Mathf.Max(0.01f, musicUserVolume)));

        if (masterLastNonZeroVolume <= 0f) masterLastNonZeroVolume = 1f;
        if (sfxLastNonZeroVolume <= 0f) sfxLastNonZeroVolume = 1f;
        if (musicLastNonZeroVolume <= 0f) musicLastNonZeroVolume = 1f;

        prefsLoaded = true;
    }

    private static void SavePrefs()
    {
        PlayerPrefs.SetFloat(PrefMasterVolume, masterUserVolume);
        PlayerPrefs.SetFloat(PrefSfxVolume, sfxUserVolume);
        PlayerPrefs.SetFloat(PrefMusicVolume, musicUserVolume);
        PlayerPrefs.SetFloat(PrefMasterLastNonZero, masterLastNonZeroVolume);
        PlayerPrefs.SetFloat(PrefSfxLastNonZero, sfxLastNonZeroVolume);
        PlayerPrefs.SetFloat(PrefMusicLastNonZero, musicLastNonZeroVolume);
        PlayerPrefs.Save();
    }

    private void ApplyAllVolumes()
    {
        ApplyMasterVolume();
        ApplyMusicVolume();
        ApplySfxVolume();
    }

    private void ApplyMasterVolume()
    {
        if (!hasMasterBus)
        {
            try
            {
                masterBus = RuntimeManager.GetBus(masterBusPath);
                hasMasterBus = masterBus.isValid();
            }
            catch (Exception) { }
        }

        if (hasMasterBus && masterBus.isValid())
        {
            masterBus.setVolume(masterUserVolume * pauseDucking);
        }
    }

    private void ApplyMusicVolume()
    {
        if (!hasMusicBus)
        {
            try
            {
                musicBus = RuntimeManager.GetBus(musicBusPath);
                hasMusicBus = musicBus.isValid();
            }
            catch (Exception) { }
        }

        if (hasMusicBus && musicBus.isValid())
        {
            musicBus.setVolume(musicUserVolume);
        }
    }

    private void ApplySfxVolume()
    {
        if (!hasSfxBus)
        {
            try
            {
                sfxBus = RuntimeManager.GetBus(sfxBusPath);
                hasSfxBus = sfxBus.isValid();
            }
            catch (Exception) { }
        }

        if (hasSfxBus && sfxBus.isValid())
        {
            sfxBus.setVolume(sfxUserVolume);
        }
    }

    // --- Volume Getters ---
    public float GetMasterVolume()
    {
        EnsurePrefsLoaded();
        return masterUserVolume;
    }

    public float GetSfxVolume()
    {
        EnsurePrefsLoaded();
        return sfxUserVolume;
    }

    public float GetMusicVolume()
    {
        EnsurePrefsLoaded();
        return musicUserVolume;
    }

    public float GetEffectiveMasterVolume()
    {
        EnsurePrefsLoaded();
        return masterUserVolume * pauseDucking;
    }

    public float GetEffectiveSfxVolume()
    {
        EnsurePrefsLoaded();
        return sfxUserVolume * pauseDucking;
    }

    public float GetEffectiveMusicVolume()
    {
        EnsurePrefsLoaded();
        return musicUserVolume * pauseDucking;
    }

    // --- Volume Setters (usados por sliders) ---
    public void SetMasterVolume(float value01)
    {
        EnsurePrefsLoaded();
        masterUserVolume = Mathf.Clamp01(value01);
        if (masterUserVolume > 0.0001f)
            masterLastNonZeroVolume = masterUserVolume;

        SavePrefs();
        ApplyMasterVolume();
    }

    public void SetSfxVolume(float value01)
    {
        EnsurePrefsLoaded();
        sfxUserVolume = Mathf.Clamp01(value01);
        if (sfxUserVolume > 0.0001f)
            sfxLastNonZeroVolume = sfxUserVolume;

        SavePrefs();
        ApplySfxVolume();
    }

    public void SetMusicVolume(float value01)
    {
        EnsurePrefsLoaded();
        musicUserVolume = Mathf.Clamp01(value01);
        if (musicUserVolume > 0.0001f)
            musicLastNonZeroVolume = musicUserVolume;

        SavePrefs();
        ApplyMusicVolume();
    }

    // --- Mute / Unmute ---
    public void MuteMaster()
    {
        EnsurePrefsLoaded();
        if (masterUserVolume > 0.0001f)
            masterLastNonZeroVolume = masterUserVolume;

        masterUserVolume = 0f;
        SavePrefs();
        ApplyMasterVolume();
    }

    public void UnmuteMasterRestore()
    {
        EnsurePrefsLoaded();
        if (masterUserVolume > 0.0001f)
            return;

        masterUserVolume = Mathf.Clamp01(Mathf.Max(0.01f, masterLastNonZeroVolume));
        SavePrefs();
        ApplyMasterVolume();
    }

    public void MuteSfx()
    {
        EnsurePrefsLoaded();
        if (sfxUserVolume > 0.0001f)
            sfxLastNonZeroVolume = sfxUserVolume;

        sfxUserVolume = 0f;
        SavePrefs();
        ApplySfxVolume();
    }

    public void UnmuteSfxRestore()
    {
        EnsurePrefsLoaded();
        if (sfxUserVolume > 0.0001f)
            return;

        sfxUserVolume = Mathf.Clamp01(Mathf.Max(0.01f, sfxLastNonZeroVolume));
        SavePrefs();
        ApplySfxVolume();
    }

    public void MuteMusic()
    {
        EnsurePrefsLoaded();
        if (musicUserVolume > 0.0001f)
            musicLastNonZeroVolume = musicUserVolume;

        musicUserVolume = 0f;
        SavePrefs();
        ApplyMusicVolume();
    }

    public void UnmuteMusicRestore()
    {
        EnsurePrefsLoaded();
        if (musicUserVolume > 0.0001f)
            return;

        musicUserVolume = Mathf.Clamp01(Mathf.Max(0.01f, musicLastNonZeroVolume));
        SavePrefs();
        ApplyMusicVolume();
    }

    // --- Pause Ducking / Global Volume ---
    public void SetGlobalVolume(float value01)
    {
        pauseDucking = Mathf.Clamp01(value01);
        ApplyMasterVolume();
    }

    public void LowerGlobalVolume(float value01 = 0.25f)
    {
        normalPauseDucking = pauseDucking;
        SetGlobalVolume(value01);
    }

    public void RestoreGlobalVolume()
    {
        SetGlobalVolume(normalPauseDucking);
    }

    #endregion

    #region Sound Playback & Instances

    public void PlayOneShot(EventReference soundEvent, Vector3 position)
    {
        if (soundEvent.IsNull)
            return;

        RuntimeManager.PlayOneShot(soundEvent, position);
    }

    public void PlayOneShot(EventReference soundEvent)
    {
        if (soundEvent.IsNull)
            return;

        RuntimeManager.PlayOneShot(soundEvent);
    }

    /// <summary>
    /// Reproduce un sonido basado en el enum SoundType mediante FMODEvents.
    /// </summary>
    public void PlaySound(SoundType sound, Vector3 position = default)
    {
        if (FMODEvents.instance == null)
            return;

        EventReference evt = FMODEvents.instance.GetEventByType(sound);
        if (!evt.IsNull)
        {
            if (position == default)
                PlayOneShot(evt);
            else
                PlayOneShot(evt, position);
        }
    }
  
    public void PlayStingerSound()
    {
        PlaySound(SoundType.DialogueStinger, default);
    }

    public EventInstance CreateEventInstance(EventReference soundEventReference)
    {
        if (soundEventReference.IsNull)
            return default;

        EventInstance eventInstance = RuntimeManager.CreateInstance(soundEventReference);
        activeEvents.Add(eventInstance);
        return eventInstance;
    }

    public EventInstance CreateEventInstance(EventReference soundEventReference, Vector3 position)
    {
        if (soundEventReference.IsNull)
            return default;

        EventInstance eventInstance = RuntimeManager.CreateInstance(soundEventReference);
        eventInstance.set3DAttributes(RuntimeUtils.To3DAttributes(position));
        activeEvents.Add(eventInstance);
        return eventInstance;
    }

    public void StopAndRelease(EventInstance eventInstance, FMOD.Studio.STOP_MODE stopMode = FMOD.Studio.STOP_MODE.ALLOWFADEOUT)
    {
        if (!eventInstance.isValid())
            return;

        eventInstance.stop(stopMode);
        eventInstance.release();
        activeEvents.Remove(eventInstance);
    }

    private void CleanUpStoppedEvents()
    {
        for (int i = activeEvents.Count - 1; i >= 0; i--)
        {
            EventInstance inst = activeEvents[i];
            if (!inst.isValid())
            {
                activeEvents.RemoveAt(i);
                continue;
            }

            inst.getPlaybackState(out PLAYBACK_STATE state);
            if (state == PLAYBACK_STATE.STOPPED)
            {
                inst.release();
                activeEvents.RemoveAt(i);
            }
        }
    }

    private void CleanUp()
    {
        foreach (EventInstance eventInstance in activeEvents)
        {
            if (eventInstance.isValid())
            {
                eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
                eventInstance.release();
            }
        }
        activeEvents.Clear();
    }

    private void OnDestroy()
    {
        CleanUp();
    }

    #endregion

    #region Static Helper Facades

    public static float SfxVolume
    {
        get => instance != null ? instance.GetSfxVolume() : sfxUserVolume;
        set { if (instance != null) instance.SetSfxVolume(value); else sfxUserVolume = Mathf.Clamp01(value); }
    }

    public static float MusicVolume
    {
        get => instance != null ? instance.GetMusicVolume() : musicUserVolume;
        set { if (instance != null) instance.SetMusicVolume(value); else musicUserVolume = Mathf.Clamp01(value); }
    }

    public static float MasterVolume
    {
        get => instance != null ? instance.GetMasterVolume() : masterUserVolume;
        set { if (instance != null) instance.SetMasterVolume(value); else masterUserVolume = Mathf.Clamp01(value); }
    }

    #endregion
}
