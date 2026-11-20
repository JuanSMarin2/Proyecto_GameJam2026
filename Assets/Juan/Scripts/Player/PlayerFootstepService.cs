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
    [Tooltip("Nombre exacto del parámetro de superficie en FMOD Studio (ej. 'Surface').")]
    [SerializeField] private string surfaceParameterName = "Surface";

    // ── IFootstepAudioService ─────────────────────────────────────────────────

    /// <summary>
    /// Dispara el evento de pasos una vez con la superficie y posición dadas.
    /// Usa CreateEventInstance → setParameter → start → release (fire-and-forget).
    /// </summary>
    public void OnPlayerStep(SurfaceType surface, Vector3 worldPosition)
    {
        // Validar que AudioManager está vivo
        if (AudioManager.instance == null)
        {
            Debug.LogWarning("[PlayerFootstepService] AudioManager.instance es null. ¿Falta en la escena?");
            return;
        }

        // Resolver el EventReference: override → singleton → abort
        EventReference evt = ResolveEventReference();
        if (evt.IsNull)
        {
            Debug.LogWarning("[PlayerFootstepService] No se encontró evento de pasos. " +
                             "Asigna uno en el Inspector o en FMODEvents.FootstepsSoundEvent.");
            return;
        }

        // Crear instancia, aplicar parámetro de superficie, disparar y liberar
        FMOD.Studio.EventInstance instance = AudioManager.instance.CreateEventInstance(evt, worldPosition);
        if (!instance.isValid()) return;

        // El valor del parámetro es el índice entero del enum (0=Default, 1=Stone, 2=Grass…)
        instance.setParameterByName(surfaceParameterName, (float)(int)surface);
        instance.start();
        instance.release(); // fire-and-forget: FMOD libera cuando termina
    }

    // ── Helpers privados ──────────────────────────────────────────────────────

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
