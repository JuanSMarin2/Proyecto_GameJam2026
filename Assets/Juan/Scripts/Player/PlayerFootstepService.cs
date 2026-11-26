using FMODUnity;
using UnityEngine;

/// <summary>
/// Implementación concreta de IFootstepAudioService que dispara el evento FMOD
/// de pasos del jugador con el parámetro de superficie correspondiente.
///
/// Colócalo en el mismo GameObject que PlayerMovement.
/// En el Inspector puedes sobreescribir el evento FMOD por defecto
/// (si lo dejas vacío usa FMODEvents.instance.FootstepsSoundEvent).
/// </summary>
public class PlayerFootstepService : MonoBehaviour, IFootstepAudioService
{
    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("FMOD Event")]
    [Tooltip("Evento FMOD de pasos. Si está vacío usa FMODEvents.instance.FootstepsSoundEvent.")]
    [SerializeField] private EventReference footstepEventOverride;

    [Header("FMOD Parameter")]
    [Tooltip("Nombre exacto del parámetro etiquetado de superficie en FMOD Studio ('Surface_type').")]
    [SerializeField] private string surfaceParameterName = "Surface_type";

    [Tooltip("Si está activo, prioriza el tipo de superficie detectado por triggers en AudioManager cuando SurfaceDetector devuelva Default.")]
    [SerializeField] private bool fallbackToAudioManagerSurface = true;

    [Tooltip("Etiqueta de respaldo si ni el detector ni AudioManager tienen una superficie definida.")]
    [SerializeField] private string defaultSurfaceLabel = "Concrete";

    // ── IFootstepAudioService ─────────────────────────────────────────────────

    /// <summary>
    /// Dispara el evento de pasos una vez con la superficie y posición dadas.
    /// Usa CreateEventInstance → setParameterByNameWithLabel → start → release.
    /// </summary>
    public void OnPlayerStep(SurfaceType surface, Vector3 worldPosition)
    {
        if (AudioManager.instance == null)
        {
            Debug.LogWarning("[PlayerFootstepService] AudioManager.instance es null. ¿Falta en la escena?");
            return;
        }

        EventReference evt = ResolveEventReference();
        if (evt.IsNull)
        {
            Debug.LogWarning("[PlayerFootstepService] No se encontró evento de pasos. " +
                             "Asigna uno en el Inspector o en FMODEvents.FootstepsSoundEvent.");
            return;
        }

        FMOD.Studio.EventInstance instance = AudioManager.instance.CreateEventInstance(evt, worldPosition);
        if (!instance.isValid()) return;

        string surfaceLabel = ResolveSurfaceLabel(surface);
        AudioManager.instance.SetEventParameterWithLabel(instance, surfaceParameterName, surfaceLabel);
        instance.start();
        instance.release(); // fire-and-forget: FMOD libera cuando termina
    }

    // ── Helpers privados ──────────────────────────────────────────────────────

    private string ResolveSurfaceLabel(SurfaceType surface)
    {
        switch (surface)
        {
            case SurfaceType.Dirt:
                return "Dirt";
            case SurfaceType.Water:
                return "Water";
            case SurfaceType.Grass:
                return "Grass";
            case SurfaceType.Stone:
            case SurfaceType.Metal:
            case SurfaceType.Wood:
                return "Concrete";
            case SurfaceType.Sand:
                return "Dirt";
            case SurfaceType.Default:
            default:
                if (fallbackToAudioManagerSurface && AudioManager.instance != null)
                {
                    string triggerSurface = AudioManager.instance.GetCurrentSurfaceType();
                    if (!string.IsNullOrEmpty(triggerSurface))
                        return triggerSurface;
                }
                return defaultSurfaceLabel;
        }
    }

    /// <summary>
    /// Prioridad: override en Inspector → FMODEvents singleton → vacío.
    /// </summary>
    private EventReference ResolveEventReference()
    {
        if (!footstepEventOverride.IsNull)
            return footstepEventOverride;

        if (FMODEvents.instance != null && !FMODEvents.instance.FootstepsSoundEvent.IsNull)
            return FMODEvents.instance.FootstepsSoundEvent;

        return default;
    }
}
