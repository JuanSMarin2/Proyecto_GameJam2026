using UnityEngine;

/// <summary>
/// Contrato que desacopla el sistema de movimiento del jugador de cualquier
/// implementación de audio concreta (FMOD, Unity Audio, mock de testing…).
/// PlayerMovement sólo conoce esta interfaz; nunca llama directamente a FMOD.
/// </summary>
public interface IFootstepAudioService
{
    /// <summary>
    /// Llamado una vez cada vez que el jugador completa el movimiento a un nuevo tile.
    /// </summary>
    /// <param name="surface">Tipo de superficie detectada bajo el jugador.</param>
    /// <param name="worldPosition">Posición mundial donde ocurrió el paso (para audio 3D).</param>
    void OnPlayerStep(SurfaceType surface, Vector3 worldPosition);
}
