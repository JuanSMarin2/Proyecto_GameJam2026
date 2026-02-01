using System;
using UnityEngine;

public class MusicLayerManager : MonoBehaviour
{
    [Header("Sounds")]
    [SerializeField] private SoundsSO soundsSO;

    [Header("Fade")]
    [SerializeField, Min(0f)] private float fadeInSeconds = 0.25f;
    [SerializeField, Min(0f)] private float fadeOutSeconds = 0.35f;

    [Header("Sound Types")]
    [SerializeField] private SoundType baseSound = SoundType.CapaBase;
    [SerializeField] private SoundType africanLayer = SoundType.CapaAFricana;
    [SerializeField] private SoundType europeanLayer = SoundType.CapaEuropea;
    [SerializeField] private SoundType asianLayer = SoundType.CapaAsiatica;
    [SerializeField] private SoundType latinLayer = SoundType.CapaLatina;

    private AudioSource baseSource;
    private AudioSource africanSource;
    private AudioSource europeanSource;
    private AudioSource asianSource;
    private AudioSource latinSource;

    private bool africanTargetActive;
    private bool europeanTargetActive;
    private bool asianTargetActive;
    private bool latinTargetActive;

    private float baseVolume;
    private float africanVolume;
    private float europeanVolume;
    private float asianVolume;
    private float latinVolume;

    private void Awake()
    {
        EnsureSources();

        if (EventManager.Instance != null)
        {
            EventManager.Instance.EnActivarCapa += ActivarCapa;
            EventManager.Instance.EnDesactivarCapa += DesactivarCapa;
        }

    }

    private void OnDisable()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.EnActivarCapa -= ActivarCapa;
            EventManager.Instance.EnDesactivarCapa -= DesactivarCapa;
        }

    }

    private void Start()
    {
        if (soundsSO == null)
        {
            Debug.LogWarning("[MusicLayerManager] Missing SoundsSO reference.");
            enabled = false;
            return;
        }

        SetupSource(baseSource, baseSound, out baseVolume);
        SetupSource(africanSource, africanLayer, out africanVolume);
        SetupSource(europeanSource, europeanLayer, out europeanVolume);
        SetupSource(asianSource, asianLayer, out asianVolume);
        SetupSource(latinSource, latinLayer, out latinVolume);

        baseSource.volume = baseVolume;
        africanSource.volume = 0f;
        europeanSource.volume = 0f;
        asianSource.volume = 0f;
        latinSource.volume = 0f;

        // Schedule playback to start aligned.
        double startTime = AudioSettings.dspTime + 0.1d;
        baseSource.PlayScheduled(startTime);
        africanSource.PlayScheduled(startTime);
        europeanSource.PlayScheduled(startTime);
        asianSource.PlayScheduled(startTime);
        latinSource.PlayScheduled(startTime);
    }

    private void Update()
    {
        if (baseSource == null || baseSource.clip == null)
            return;

        // Keep base steady; fade layers towards their targets.
        baseSource.volume = baseVolume;

        UpdateLayerVolume(africanSource, africanTargetActive, africanVolume);
        UpdateLayerVolume(europeanSource, europeanTargetActive, europeanVolume);
        UpdateLayerVolume(asianSource, asianTargetActive, asianVolume);
        UpdateLayerVolume(latinSource, latinTargetActive, latinVolume);
    }

    private void ActivarCapa(int capa)
    {
        switch (capa)
        {
            case 1:
                africanTargetActive = true;
                break;
            case 2:
                europeanTargetActive = true;
                break;
            case 3:
                asianTargetActive = true;
                break;
            case 4:
                latinTargetActive = true;
                break;
        }
    }

    private void DesactivarCapa(int capa)
    {
        switch (capa)
        {
            case 1:
                africanTargetActive = false;
                break;
            case 2:
                europeanTargetActive = false;
                break;
            case 3:
                asianTargetActive = false;
                break;
            case 4:
                latinTargetActive = false;
                break;
        }
    }

    private void UpdateLayerVolume(AudioSource source, bool targetActive, float activeVolume)
    {
        if (source == null)
            return;

        float targetVolume = targetActive ? activeVolume : 0f;
        float seconds = targetActive ? fadeInSeconds : fadeOutSeconds;

        if (seconds <= 0f)
        {
            source.volume = targetVolume;
            return;
        }

        // Speed is in "volume units per second".
        float maxDelta = Mathf.Max(activeVolume, 1f) / Mathf.Max(seconds, 0.0001f) * Time.deltaTime;
        source.volume = Mathf.MoveTowards(source.volume, targetVolume, maxDelta);
    }

    private void EnsureSources()
    {
        if (baseSource == null) baseSource = gameObject.AddComponent<AudioSource>();
        if (africanSource == null) africanSource = gameObject.AddComponent<AudioSource>();
        if (europeanSource == null) europeanSource = gameObject.AddComponent<AudioSource>();
        if (asianSource == null) asianSource = gameObject.AddComponent<AudioSource>();
        if (latinSource == null) latinSource = gameObject.AddComponent<AudioSource>();
    }

    private void SetupSource(AudioSource source, SoundType soundType, out float configuredVolume)
    {
        configuredVolume = 1f;

        if (source == null)
            return;

        source.playOnAwake = false;
        source.loop = true;

        int index = (int)soundType;
        if (soundsSO.sounds == null || index < 0 || index >= soundsSO.sounds.Length)
        {
            Debug.LogWarning($"[MusicLayerManager] SoundsSO missing entry for {soundType}.");
            source.clip = null;
            return;
        }

        SoundList list = soundsSO.sounds[index];
        configuredVolume = list.volume;
        source.outputAudioMixerGroup = list.mixer;

        if (list.sounds == null || list.sounds.Length == 0)
        {
            Debug.LogWarning($"[MusicLayerManager] No clips assigned for {soundType}.");
            source.clip = null;
            return;
        }

        // For music we use the first clip deterministically.
        source.clip = list.sounds[0];
    }
}
