using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Clase base para zonas Trigger 2D de audio acoplables a colliders del escenario.
/// Evita duplicar la detección de entrada/salida del jugador entre zonas de ubicación y superficie.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public abstract class AudioTriggerZoneBase : MonoBehaviour
{
    [Header("Trigger Detection")]
    [Tooltip("Tag del jugador que activa esta zona.")]
    [SerializeField] protected string playerTag = "Player";

    [Tooltip("Si está activo, usa directamente el tag de este GameObject en lugar del selector del Inspector.")]
    [SerializeField] protected bool useObjectTagAsValue = false;

    [Tooltip("Si está activo, al salir del trigger vuelve al valor de respaldo configurado.")]
    [SerializeField] protected bool revertOnExit = false;

    [Header("Optional Events")]
    [SerializeField] protected UnityEvent onPlayerEnter;
    [SerializeField] protected UnityEvent onPlayerExit;

    protected virtual void Reset()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.isTrigger = true;
    }

    private void Start()
    {
        CheckForCollision2D();
    }

    private void CheckForCollision2D()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null && col.isTrigger)
        {
            Collider2D[] overlappingColliders = Physics2D.OverlapBoxAll(col.bounds.center, col.bounds.size, 0f);
            foreach (Collider2D other in overlappingColliders)
            {
                if (other != null && other.CompareTag(playerTag))
                {
                    string targetValue = useObjectTagAsValue ? gameObject.tag : GetConfiguredEnterValue();
                    ApplyValue(targetValue);
                    onPlayerEnter?.Invoke();
                    break;
                }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null || !other.CompareTag(playerTag))
            return;

        string targetValue = useObjectTagAsValue ? gameObject.tag : GetConfiguredEnterValue();
        ApplyValue(targetValue);
        onPlayerEnter?.Invoke();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other == null || !other.CompareTag(playerTag))
            return;

        if (revertOnExit)
            ApplyValue(GetConfiguredExitValue());

        onPlayerExit?.Invoke();
    }

    protected abstract string GetConfiguredEnterValue();
    protected abstract string GetConfiguredExitValue();
    protected abstract void ApplyValue(string value);
}
