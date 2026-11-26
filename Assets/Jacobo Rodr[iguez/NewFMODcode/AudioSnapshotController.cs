using FMOD.Studio;
using FMODUnity;
using UnityEngine;

/// <summary>
/// Controla un Snapshot de FMOD Studio (por defecto "snapshot:/TiendaAbierta").
/// Puede acoplarse a cualquier objeto y conectarse desde UnityEvents
/// (como AyudaGuia.OnHelpActivated o AudioSurfaceTrigger.onPlayerEnter).
/// </summary>
public class AudioSnapshotController : MonoBehaviour
{
    [Header("FMOD Snapshot")]
    [Tooltip("Opcional: asigna el Snapshot desde el selector de FMOD. Si está vacío, se usará 'snapshotPath'.")]
    [SerializeField] private EventReference snapshotEvent;

    [Tooltip("Ruta del Snapshot en FMOD Studio si no se asignó EventReference.")]
    [SerializeField] private string snapshotPath = "snapshot:/TiendaAbierta";

    [Header("Lifecycle")]
    [SerializeField] private bool applyOnStart = false;
    [SerializeField] private bool allowFadeoutOnStop = true;

    private EventInstance snapshotInstance;

    private void Start()
    {
        if (applyOnStart)
            ApplySnapshot();
    }

    /// <summary>
    /// Activa el snapshot configurado (idempotente: no lo duplica si ya está activo).
    /// </summary>
    public void ApplySnapshot()
    {
        if (!RuntimeManager.IsInitialized)
            return;

        if (snapshotInstance.isValid())
        {
            snapshotInstance.getPlaybackState(out PLAYBACK_STATE state);
            if (state == PLAYBACK_STATE.PLAYING || state == PLAYBACK_STATE.STARTING)
                return;
        }
        else
        {
            snapshotInstance = CreateSnapshotInstance();
        }

        if (snapshotInstance.isValid())
            snapshotInstance.start();
    }

    /// <summary>
    /// Detiene y libera el snapshot activo.
    /// </summary>
    public void StopSnapshot()
    {
        if (!snapshotInstance.isValid())
            return;

        FMOD.Studio.STOP_MODE mode = allowFadeoutOnStop
            ? FMOD.Studio.STOP_MODE.ALLOWFADEOUT
            : FMOD.Studio.STOP_MODE.IMMEDIATE;

        if (AudioManager.instance != null)
        {
            AudioManager.instance.StopAndRelease(snapshotInstance, mode);
        }
        else
        {
            snapshotInstance.stop(mode);
            snapshotInstance.release();
        }

        snapshotInstance.clearHandle();
    }

    public void SetSnapshotActive(bool active)
    {
        if (active)
            ApplySnapshot();
        else
            StopSnapshot();
    }

    private EventInstance CreateSnapshotInstance()
    {
        if (!snapshotEvent.IsNull)
        {
            return AudioManager.instance != null
                ? AudioManager.instance.CreateEventInstance(snapshotEvent)
                : RuntimeManager.CreateInstance(snapshotEvent);
        }

        if (!string.IsNullOrWhiteSpace(snapshotPath))
            return RuntimeManager.CreateInstance(snapshotPath);

        Debug.LogWarning("[AudioSnapshotController] No se asignó ningún Snapshot ni ruta válida.");
        return default;
    }

    private void OnDestroy()
    {
        StopSnapshot();
    }
}
