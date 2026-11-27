using UnityEngine;

/// <summary>
/// Disparador reutilizable para parámetros tipo "Trigger" cuantizados en FMOD Studio
/// (como PuzzleCompleted o Victory): lleva el parámetro de 0 a 1 durante 'holdSeconds'
/// para que la cuantización de FMOD tome la transición, y luego lo devuelve a 0.
/// Puede activarse por Trigger 2D o llamando a TriggerStinger() desde UnityEvents/código.
/// </summary>
public class AudioStingerTrigger : MonoBehaviour
{
    public enum StingerType
    {
        PuzzleCompleted,
        Victory,
        Custom
    }

    [Header("Stinger Configuration")]
    [SerializeField] private StingerType stingerType = StingerType.PuzzleCompleted;
    [Tooltip("Sólo se usa si Stinger Type es Custom.")]
    [SerializeField] private string customParameterName = "PuzzleCompleted";

    [Tooltip("Tiempo en segundos que el parámetro permanece en 1 antes de volver a 0 para que FMOD tome la cuantización.")]
    [SerializeField, Min(0f)] private float holdSeconds = 0.7f;

    [Header("2D Trigger Options")]
    [SerializeField] private bool triggerOnPlayerEnter2D = true;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private bool triggerOnce = true;

    private bool hasTriggered;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!triggerOnPlayerEnter2D)
            return;

        if (other == null || !other.CompareTag(playerTag))
            return;

        TriggerStinger();
    }

    /// <summary>
    /// Activa el pulso del stinger en AudioManager (de 0 a 1 durante holdSeconds, luego a 0).
    /// </summary>
    public void TriggerStinger()
    {
        Debug.Log($"[AudioStingerTrigger] Triggering stinger: {stingerType} (Custom: {customParameterName})");
        if (triggerOnce && hasTriggered)
            return;

        if (AudioManager.instance == null)
        {
            Debug.LogWarning("[AudioStingerTrigger] AudioManager.instance es null.");
            return;
        }

        hasTriggered = true;

        switch (stingerType)
        {
            case StingerType.PuzzleCompleted:
                AudioManager.instance.TriggerPuzzleCompletedStinger(holdSeconds);
                break;
            case StingerType.Victory:
                AudioManager.instance.TriggerVictoryStinger(holdSeconds);
                break;
            case StingerType.Custom:
                AudioManager.instance.TriggerParameterPulse(customParameterName, holdSeconds);
                break;
        }
    }

    public void ResetTrigger()
    {
        hasTriggered = false;
    }
}
