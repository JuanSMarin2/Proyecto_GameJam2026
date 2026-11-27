using System;
using System.Collections;
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
    [SerializeField] private string ambienceBusPath = "bus:/Ambience";

    [Header("FMOD Parameter Names")]
    [SerializeField] private string pauseParameterName = "IsPaused";
    [SerializeField] private string locationParameterName = "Location";
    [SerializeField] private string defaultLocationLabel = "Normal";
    [SerializeField] private string surfaceParameterName = "Surface_type";
    [SerializeField] private string defaultSurfaceLabel = "Concrete";

    [Header("FMOD Quantized Stinger Triggers")]
    [SerializeField] private string victoryParameterName = "Victory";
    [SerializeField, Min(0f)] private float victoryHoldSeconds = 0.7f;
    [SerializeField] private string puzzleCompletedParameterName = "PuzzleCompleted";
    [SerializeField, Min(0f)] private float puzzleCompletedHoldSeconds = 0.7f;

    private const string PrefMasterVolume = "audio.master.volume";
    private const string PrefSfxVolume = "audio.sfx.volume";
    private const string PrefMusicVolume = "audio.music.volume";
    private const string PrefMasterLastNonZero = "audio.master.lastNonZero";
    private const string PrefSfxLastNonZero = "audio.sfx.lastNonZero";
    private const string PrefMusicLastNonZero = "audio.music.lastNonZero";
    private const string PrefAmbienceVolume = "audio.ambience.volume";
    private const string PrefAmbienceLastNonZero = "audio.ambience.lastNonZero";
    

    private static bool prefsLoaded;
    private static float masterUserVolume = 1f;
    private static float sfxUserVolume = 1f;
    private static float musicUserVolume = 1f;
    private static float ambienceUserVolume = 1f;
    private static float masterLastNonZeroVolume = 1f;
    private static float sfxLastNonZeroVolume = 1f;
    private static float musicLastNonZeroVolume = 1f;
    private static float ambienceLastNonZeroVolume = 1f;
    

    private Bus masterBus;
    private Bus musicBus;
    private Bus sfxBus;
    private Bus ambienceBus;
    private bool hasMasterBus;
    private bool hasMusicBus;
    private bool hasSfxBus;
    private bool hasAmbienceBus;

    private readonly List<EventInstance> activeEvents = new List<EventInstance>();
    private readonly Dictionary<string, Coroutine> activePulseCoroutines = new Dictionary<string, Coroutine>(StringComparer.Ordinal);

    private bool currentPausedState;
    private bool hasInitializedPausedState;
    private string currentLocationLabel;
    private string currentSurfaceLabel;

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
        InitializeDefaultParameters();
    }

    private void InitializeDefaultParameters()
    {
        SetPausedState(false);

        if (!string.IsNullOrWhiteSpace(defaultLocationLabel))
            SetLocation(defaultLocationLabel);

        if (!string.IsNullOrWhiteSpace(defaultSurfaceLabel))
            currentSurfaceLabel = defaultSurfaceLabel;
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
        try
        {
            var ambienceBus = RuntimeManager.GetBus(ambienceBusPath);
            this.ambienceBus = ambienceBus;
            hasAmbienceBus = ambienceBus.isValid();
        }
        catch (Exception)
        {
            hasAmbienceBus = false;
        } 
    }

    private static void EnsurePrefsLoaded()
    {
        if (prefsLoaded)
            return;

        masterUserVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(PrefMasterVolume, 1f));
        sfxUserVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(PrefSfxVolume, 1f));
        musicUserVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(PrefMusicVolume, 1f));
        ambienceUserVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(PrefAmbienceVolume, 1f));
        masterLastNonZeroVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(PrefMasterLastNonZero, Mathf.Max(0.01f, masterUserVolume)));
        sfxLastNonZeroVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(PrefSfxLastNonZero, Mathf.Max(0.01f, sfxUserVolume)));
        musicLastNonZeroVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(PrefMusicLastNonZero, Mathf.Max(0.01f, musicUserVolume)));
        ambienceLastNonZeroVolume = Mathf.Clamp01(PlayerPrefs.GetFloat(PrefAmbienceLastNonZero, Mathf.Max(0.01f, ambienceUserVolume)));

        if (masterLastNonZeroVolume <= 0f) masterLastNonZeroVolume = 1f;
        if (sfxLastNonZeroVolume <= 0f) sfxLastNonZeroVolume = 1f;
        if (musicLastNonZeroVolume <= 0f) musicLastNonZeroVolume = 1f;
        if (ambienceLastNonZeroVolume <= 0f) ambienceLastNonZeroVolume = 1f;

        prefsLoaded = true;
    }

    private static void SavePrefs()
    {
        PlayerPrefs.SetFloat(PrefMasterVolume, masterUserVolume);
        PlayerPrefs.SetFloat(PrefSfxVolume, sfxUserVolume);
        PlayerPrefs.SetFloat(PrefMusicVolume, musicUserVolume);
        PlayerPrefs.SetFloat(PrefAmbienceVolume, ambienceUserVolume);
        PlayerPrefs.SetFloat(PrefMasterLastNonZero, masterLastNonZeroVolume);
        PlayerPrefs.SetFloat(PrefSfxLastNonZero, sfxLastNonZeroVolume);
        PlayerPrefs.SetFloat(PrefMusicLastNonZero, musicLastNonZeroVolume);
        PlayerPrefs.SetFloat(PrefAmbienceLastNonZero, ambienceLastNonZeroVolume);
        PlayerPrefs.Save();
    }

    private void ApplyAllVolumes()
    {
        ApplyMasterVolume();
        ApplyMusicVolume();
        ApplySfxVolume();
        ApplyAmbienceVolume();
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
            masterBus.setVolume(masterUserVolume);
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
    private void ApplyAmbienceVolume()
    {
        if (!hasAmbienceBus)
        {
            try
            {
                ambienceBus = RuntimeManager.GetBus(ambienceBusPath);
                hasAmbienceBus = ambienceBus.isValid();
            }
            catch (Exception) { }
        }

        if (hasAmbienceBus && ambienceBus.isValid())
        {
            ambienceBus.setVolume(ambienceUserVolume);
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
    public float GetAmbienceVolume()
    {
        EnsurePrefsLoaded();
        return ambienceUserVolume;
    }

    public float GetEffectiveMasterVolume()
    {
        EnsurePrefsLoaded();
        return masterUserVolume;
    }

    public float GetEffectiveSfxVolume()
    {
        EnsurePrefsLoaded();
        return sfxUserVolume;
    }

    public float GetEffectiveMusicVolume()
    {
        EnsurePrefsLoaded();
        return musicUserVolume;
    }

    public float GetEffectiveAmbienceVolume()
    {
        EnsurePrefsLoaded();
        return ambienceUserVolume;
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
    public void SetAmbienceVolume(float value01)
    {
        EnsurePrefsLoaded();
        ambienceUserVolume = Mathf.Clamp01(value01);
        if (ambienceUserVolume > 0.0001f)
            ambienceLastNonZeroVolume = ambienceUserVolume;

        SavePrefs();
        ApplyAmbienceVolume();
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
    public void MuteAmbience()
    {
        EnsurePrefsLoaded();
        if (ambienceUserVolume > 0.0001f)
            ambienceLastNonZeroVolume = ambienceUserVolume;

        ambienceUserVolume = 0f;
        SavePrefs();
        ApplyAmbienceVolume();
    }

    public void UnmuteAmbienceRestore()
    {
        EnsurePrefsLoaded();
        if (ambienceUserVolume > 0.0001f)
            return;

        ambienceUserVolume = Mathf.Clamp01(Mathf.Max(0.01f, ambienceLastNonZeroVolume));
        SavePrefs();
        ApplyAmbienceVolume();
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
 /*    public void PlaySound(SoundType sound, Vector3 position = default)
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
    } */

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
        StopAllCoroutines();
        activePulseCoroutines.Clear();
        CleanUp();
    }

    #endregion

    #region FMOD Parameters & Triggers

    /// <summary>
    /// Asigna un parámetro global numérico de FMOD Studio.
    /// </summary>
    public bool SetGlobalParameter(string parameterName, float value, bool ignoreSeekSpeed = false)
    {
        if (string.IsNullOrWhiteSpace(parameterName) || !RuntimeManager.IsInitialized)
            return false;

        return RuntimeManager.StudioSystem.setParameterByName(parameterName, value, ignoreSeekSpeed) == FMOD.RESULT.OK;
    }

    /// <summary>
    /// Asigna un parámetro global etiquetado (Labeled) de FMOD Studio.
    /// </summary>
    public bool SetGlobalParameterWithLabel(string parameterName, string label, bool ignoreSeekSpeed = false)
    {
        if (string.IsNullOrWhiteSpace(parameterName) || string.IsNullOrWhiteSpace(label) || !RuntimeManager.IsInitialized)
            return false;

        return RuntimeManager.StudioSystem.setParameterByNameWithLabel(parameterName, label, ignoreSeekSpeed) == FMOD.RESULT.OK;
    }

    /// <summary>
    /// Asigna un parámetro local numérico en una EventInstance concreta.
    /// </summary>
    public bool SetEventParameter(EventInstance eventInstance, string parameterName, float value, bool ignoreSeekSpeed = false)
    {
        if (!eventInstance.isValid() || string.IsNullOrWhiteSpace(parameterName))
            return false;

        return eventInstance.setParameterByName(parameterName, value, ignoreSeekSpeed) == FMOD.RESULT.OK;
    }

    /// <summary>
    /// Asigna un parámetro local etiquetado (Labeled) en una EventInstance concreta.
    /// </summary>
    public bool SetEventParameterWithLabel(EventInstance eventInstance, string parameterName, string label, bool ignoreSeekSpeed = false)
    {
        if (!eventInstance.isValid() || string.IsNullOrWhiteSpace(parameterName) || string.IsNullOrWhiteSpace(label))
            return false;

        return eventInstance.setParameterByNameWithLabel(parameterName, label, ignoreSeekSpeed) == FMOD.RESULT.OK;
    }

    /// <summary>
    /// Método unificado para asignar un parámetro numérico por nombre y valor.
    /// Si existe como parámetro #global en FMOD Studio lo actualiza globalmente,
    /// y también actualiza cualquier EventInstance activa gestionada por AudioManager que posea dicho parámetro.
    /// </summary>
    public void SetParameter(string parameterName, float value, bool ignoreSeekSpeed = false)
    {
        if (string.IsNullOrWhiteSpace(parameterName) || !RuntimeManager.IsInitialized)
            return;

        if (IsGlobalParameter(parameterName))
            RuntimeManager.StudioSystem.setParameterByName(parameterName, value, ignoreSeekSpeed);

        ApplyToMatchingActiveEvents(parameterName, inst => inst.setParameterByName(parameterName, value, ignoreSeekSpeed));
    }

    /// <summary>
    /// Método unificado para asignar un parámetro etiquetado (Labeled) por nombre y etiqueta.
    /// </summary>
    public void SetParameterWithLabel(string parameterName, string label, bool ignoreSeekSpeed = false)
    {
        if (string.IsNullOrWhiteSpace(parameterName) || string.IsNullOrWhiteSpace(label) || !RuntimeManager.IsInitialized)
            return;

        if (IsGlobalParameter(parameterName))
            RuntimeManager.StudioSystem.setParameterByNameWithLabel(parameterName, label, ignoreSeekSpeed);

        ApplyToMatchingActiveEvents(parameterName, inst => inst.setParameterByNameWithLabel(parameterName, label, ignoreSeekSpeed));
    }

    /// <summary>
    /// Actualiza el parámetro discreto global de pausa (1 = pausado, 0 = reanudado).
    /// </summary>
    public void SetPausedState(bool paused)
    {
        if (hasInitializedPausedState && currentPausedState == paused)
            return;

        currentPausedState = paused;
        hasInitializedPausedState = true;
        SetGlobalParameter(pauseParameterName, paused ? 1f : 0f);
    }

    /// <summary>
    /// Actualiza la ubicación global (#global Location: Normal, Tienda, PreBoss) sólo si cambió.
    /// </summary>
    public void SetLocation(string locationLabel)
    {
        if (string.IsNullOrWhiteSpace(locationLabel))
            return;

        if (string.Equals(currentLocationLabel, locationLabel, StringComparison.Ordinal))
            return;

        currentLocationLabel = locationLabel;
        SetGlobalParameterWithLabel(locationParameterName, locationLabel);
    }

    /// <summary>
    /// Actualiza el tipo de superficie actual (Surface_type: Dirt, Water, Concrete, Grass) sólo si cambió.
    /// </summary>
    public void SetSurfaceType(string surfaceLabel)
    {
        if (string.IsNullOrWhiteSpace(surfaceLabel))
            return;

        if (string.Equals(currentSurfaceLabel, surfaceLabel, StringComparison.Ordinal))
            return;

        currentSurfaceLabel = surfaceLabel;
        SetParameterWithLabel(surfaceParameterName, surfaceLabel);
    }

    public string GetCurrentLocation()
    {
        return !string.IsNullOrEmpty(currentLocationLabel) ? currentLocationLabel : defaultLocationLabel;
    }

    public string GetCurrentSurfaceType()
    {
        return !string.IsNullOrEmpty(currentSurfaceLabel) ? currentSurfaceLabel : defaultSurfaceLabel;
    }

    /// <summary>
    /// Dispara un parámetro tipo "Trigger" (0 -> activeValue durante holdSeconds -> resetValue)
    /// para transiciones cuantizadas en FMOD Studio.
    /// </summary>
    public void TriggerParameterPulse(string parameterName, float holdSeconds, float activeValue = 1f, float resetValue = 0f)
    {
        if (string.IsNullOrWhiteSpace(parameterName))
            return;

        if (activePulseCoroutines.TryGetValue(parameterName, out Coroutine running) && running != null)
            StopCoroutine(running);
    Debug.Log($"[AudioManager] Triggering parameter pulse: {parameterName} (active: {activeValue}, hold: {holdSeconds}s, reset: {resetValue})");
        activePulseCoroutines[parameterName] = StartCoroutine(
            ParameterPulseRoutine(parameterName, Mathf.Max(0f, holdSeconds), activeValue, resetValue));
    }

    public void TriggerVictoryStinger(float? customHoldSeconds = null)
    {
        float hold = customHoldSeconds ?? victoryHoldSeconds;
        TriggerParameterPulse(victoryParameterName, hold);
    }

    public void TriggerPuzzleCompletedStinger(float? customHoldSeconds = null)
    {
        float hold = customHoldSeconds ?? puzzleCompletedHoldSeconds;
        TriggerParameterPulse(puzzleCompletedParameterName, hold);
    }

    private IEnumerator ParameterPulseRoutine(string parameterName, float holdSeconds, float activeValue, float resetValue)
    {
        SetParameter(parameterName, activeValue);

        if (holdSeconds > 0f)
            yield return new WaitForSecondsRealtime(holdSeconds);

        SetParameter(parameterName, resetValue);
        activePulseCoroutines.Remove(parameterName);
    }

    private static bool IsGlobalParameter(string parameterName)
    {
        return RuntimeManager.StudioSystem.getParameterDescriptionByName(parameterName, out _) == FMOD.RESULT.OK;
    }

    private void ApplyToMatchingActiveEvents(string parameterName, Action<EventInstance> applyAction)
    {
        CleanUpStoppedEvents();

        for (int i = 0; i < activeEvents.Count; i++)
        {
            EventInstance inst = activeEvents[i];
            if (!inst.isValid())
                continue;

            if (inst.getDescription(out EventDescription desc) == FMOD.RESULT.OK &&
                desc.getParameterDescriptionByName(parameterName, out _) == FMOD.RESULT.OK)
            {
                applyAction(inst);
            }
        }
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
    public static float AmbienceVolume
    {
        get => instance != null ? instance.GetAmbienceVolume() : ambienceUserVolume;
        set { if (instance != null) instance.SetAmbienceVolume(value); else ambienceUserVolume = Mathf.Clamp01(value); }
    }

    #endregion
}
