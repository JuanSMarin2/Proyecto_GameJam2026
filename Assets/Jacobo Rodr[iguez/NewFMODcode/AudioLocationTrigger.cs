using UnityEngine;

/// <summary>
/// Zona Trigger 2D para actualizar el parámetro global 'Location' en FMOD Studio
/// (Normal, Tienda, PreBoss) al entrar el jugador.
/// </summary>
public class AudioLocationTrigger : AudioTriggerZoneBase
{
    public enum FmodLocationType
    {
        Normal,
        Tienda,
        PreBoss
    }

    [Header("FMOD Location")]
    [SerializeField] private FmodLocationType locationOnEnter = FmodLocationType.Normal;
    [SerializeField] private FmodLocationType locationOnExit = FmodLocationType.Normal;

    [SerializeField] private bool applyOnStart = true;

    void Start()
    {
        if (applyOnStart)
        ApplyValue(GetConfiguredEnterValue());
    }

    protected override string GetConfiguredEnterValue() => locationOnEnter.ToString();
    protected override string GetConfiguredExitValue() => locationOnExit.ToString();

    protected override void ApplyValue(string value)
    {
        if (AudioManager.instance != null)
            AudioManager.instance.SetLocation(value);
    }
}
