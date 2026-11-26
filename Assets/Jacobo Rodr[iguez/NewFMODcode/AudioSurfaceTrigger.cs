using UnityEngine;

/// <summary>
/// Zona Trigger 2D para actualizar el parámetro 'Surface_type' en FMOD Studio
/// (Dirt, Water, Concrete, Grass) al entrar el jugador.
/// </summary>
public class AudioSurfaceTrigger : AudioTriggerZoneBase
{
    public enum FmodSurfaceType
    {
        Dirt,
        Water,
        Concrete,
        Grass
    }

    [Header("FMOD Surface Type")]
    [SerializeField] private FmodSurfaceType surfaceOnEnter = FmodSurfaceType.Concrete;
    [SerializeField] private FmodSurfaceType surfaceOnExit = FmodSurfaceType.Concrete;

    protected override string GetConfiguredEnterValue() => surfaceOnEnter.ToString();
    protected override string GetConfiguredExitValue() => surfaceOnExit.ToString();

    protected override void ApplyValue(string value)
    {
        if (AudioManager.instance != null)
            AudioManager.instance.SetSurfaceType(value);
    }
}
