using System;
using UnityEngine;
using FMODUnity;
using FMOD.Studio;

/// <summary>
/// Gestor de música adaptativa por capas mediante FMOD Studio para MasKeeper.
/// 
/// Reemplaza el antiguo sistema de múltiples AudioSources nativos y PlayScheduled.
/// En FMOD Studio, las pistas de las capas (Base, Africana, Europea, Asiática, Latina)
/// se sincronizan automáticamente dentro de un mismo Evento en bucle.
/// 
/// El control de activación de capas se realiza a través de parámetros de FMOD (0.0f a 1.0f)
/// vinculados a los eventos de EventManager (EnActivarCapa / EnDesactivarCapa).
/// El volumen global y ducking de la música se delegan a la arquitectura de Buses de FMOD
/// (gestionados por AudioManager).
/// </summary>
public class MusicLayerManager : MonoBehaviour
{
    // ── FMOD Event ─────────────────────────────────────────────────────────────
    [Header("FMOD Music Event")]
    [Tooltip("Evento FMOD que contiene la música con las pistas de las capas sincronizadas.")]
    [SerializeField] private EventReference musicEvent;

    [Tooltip("Si está activo, la música comienza a reproducirse automáticamente en Start.")]
    [SerializeField] private bool playOnStart = true;

    // ── Parámetros de Capas FMOD ───────────────────────────────────────────────
    [Header("FMOD Layer Parameter Names")]
    [Tooltip("Capa 1: Nombre exacto del parámetro en FMOD Studio para la máscara africana.")]
    [SerializeField] private string africanParam = "Mask_01_African";

    [Tooltip("Capa 2: Nombre exacto del parámetro en FMOD Studio para la máscara europea.")]
    [SerializeField] private string europeanParam = "Mask_02_European";

    [Tooltip("Capa 3: Nombre exacto del parámetro en FMOD Studio para la máscara asiática.")]
    [SerializeField] private string asianParam = "Mask_03_Asian";

    [Tooltip("Capa 4: Nombre exacto del parámetro en FMOD Studio para la máscara latina.")]
    [SerializeField] private string latinParam = "Mask_04_Latin";

    // ── Estado Inicial ─────────────────────────────────────────────────────────
    [Header("Initial Layer States")]
    [SerializeField] private bool startAfricanActive = false;
    [SerializeField] private bool startEuropeanActive = false;
    [SerializeField] private bool startAsianActive = false;
    [SerializeField] private bool startLatinActive = false;

    // ── Variables Internas ─────────────────────────────────────────────────────
    private EventInstance musicInstance;
    private bool isSubscribedToEvents;

    // Objetivos de activación (0f = inactivo, 1f = activo)
    private float targetAfrican;
    private float targetEuropean;
    private float targetAsian;
    private float targetLatin;



    // ── Ciclo de Vida ──────────────────────────────────────────────────────────

    private void Awake()
    {
        // Inicializar estados objetivo
        targetAfrican = startAfricanActive ? 1f : 0f;
        targetEuropean = startEuropeanActive ? 1f : 0f;
        targetAsian = startAsianActive ? 1f : 0f;
        targetLatin = startLatinActive ? 1f : 0f;

        musicEvent = !musicEvent.IsNull ? musicEvent : (FMODEvents.instance != null ? FMODEvents.instance.LayerMusicSoundEvent : default);
    }

    private void OnEnable()
    {
        TrySubscribeEvents();
    }

    private void Start()
    {
        // Reintentar suscripción por si EventManager.Instance se inicializó en el Awake de otro objeto
        TrySubscribeEvents();

        InitializeAndPlayMusic();
    }


    private void OnDisable()
    {
        UnsubscribeEvents();
    }

    private void OnDestroy()
    {
        StopAndReleaseMusic();
    }

    // ── FMOD Music Playback & Lifecycle ───────────────────────────────────────

    private void InitializeAndPlayMusic()
    {
        // Resolver EventReference: el configurado en Inspector o fallback a FMODEvents
        EventReference evt = musicEvent;
        if (evt.IsNull && FMODEvents.instance != null)
        {
            evt = FMODEvents.instance.LayerMusicSoundEvent;
        }

        if (evt.IsNull)
        {
            Debug.LogWarning("[MusicLayerManager] No hay ningún evento de música asignado ni en el Inspector ni en FMODEvents.");
            return;
        }

        // Crear instancia del evento
        if (AudioManager.instance != null)
        {
            musicInstance = AudioManager.instance.CreateEventInstance(evt, transform.position);
        }
        else
        {
            musicInstance = RuntimeManager.CreateInstance(evt);
        }

        if (!musicInstance.isValid())
        {
            Debug.LogError("[MusicLayerManager] No se pudo crear la EventInstance de música de FMOD.");
            return;
        }

        // Aplicar estados iniciales de las capas
        ApplyLayerParameter(africanParam, targetAfrican);
        ApplyLayerParameter(europeanParam, targetEuropean);
        ApplyLayerParameter(asianParam, targetAsian);
        ApplyLayerParameter(latinParam, targetLatin);

        // Iniciar reproducción si está configurado
        if (playOnStart)
        {
            PlayMusic();
        }
    }

    /// <summary>
    /// Inicia la reproducción del evento musical.
    /// </summary>
    public void PlayMusic()
    {
        if (!musicInstance.isValid()) return;

        musicInstance.getPlaybackState(out PLAYBACK_STATE playState);

        if (playState != PLAYBACK_STATE.PLAYING)
        {
            musicInstance.start();
        }
    }

    /// <summary>
    /// Detiene la música con fade out configurado en el evento de FMOD y libera la instancia.
    /// </summary>
    public void StopAndReleaseMusic(bool allowFadeout = true)
    {
        if (!musicInstance.isValid()) return;

        FMOD.Studio.STOP_MODE stopMode = allowFadeout
            ? FMOD.Studio.STOP_MODE.ALLOWFADEOUT
            : FMOD.Studio.STOP_MODE.IMMEDIATE;

        if (AudioManager.instance != null)
        {
            AudioManager.instance.StopAndRelease(musicInstance, stopMode);
        }
        else
        {
            musicInstance.stop(stopMode);
            musicInstance.release();
        }

        musicInstance.clearHandle();
    }

    // ── Reacción a Capas (EventManager) ───────────────────────────────────────

    private void ActivarCapa(int capa)
    {
        SetLayerState(capa, 1f);
    }

    private void DesactivarCapa(int capa)
    {
        SetLayerState(capa, 0f);
    }

    private void SetLayerState(int capa, float value)
    {
        switch (capa)
        {
            case 1:
                targetAfrican = value;
                ApplyLayerParameter(africanParam, targetAfrican);
                break;
            case 2:
                targetEuropean = value;
                ApplyLayerParameter(europeanParam, targetEuropean);
                break;
            case 3:
                targetAsian = value;
                ApplyLayerParameter(asianParam, targetAsian);
                break;
            case 4:
                targetLatin = value;
                ApplyLayerParameter(latinParam, targetLatin);
                break;
            default:
                Debug.LogWarning($"[MusicLayerManager] Capa {capa} no reconocida (solo 1 a 4).");
                break;
        }
    }

    // ── Suavizado en Unity (Modo Manual Fallback) ──────────────────────────────

    private void ApplyLayerParameter(string paramName, float value)
    {
        if (string.IsNullOrEmpty(paramName) || !musicInstance.isValid())
            return;

        musicInstance.setParameterByName(paramName, value);
    }

    // ── Suscripción Segura a EventManager ──────────────────────────────────────

    private void TrySubscribeEvents()
    {
        if (isSubscribedToEvents) return;
        if (EventManager.Instance == null) return;

        EventManager.Instance.EnActivarCapa += ActivarCapa;
        EventManager.Instance.EnDesactivarCapa += DesactivarCapa;
        isSubscribedToEvents = true;
    }

    private void UnsubscribeEvents()
    {
        if (!isSubscribedToEvents) return;
        if (EventManager.Instance == null) return;

        EventManager.Instance.EnActivarCapa -= ActivarCapa;
        EventManager.Instance.EnDesactivarCapa -= DesactivarCapa;
        isSubscribedToEvents = false;
    }
}
