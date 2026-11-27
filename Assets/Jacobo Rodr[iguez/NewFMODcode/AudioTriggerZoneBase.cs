using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Clase base para zonas Trigger 2D de audio acoplables a colliders del escenario.
/// Soporta zonas anidadas o superpuestas (por ejemplo, un lago dentro de césped o una tienda dentro de Normal)
/// manteniendo una pila activa por tipo de zona y validando al salir si el jugador sigue dentro de otra zona.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public abstract class AudioTriggerZoneBase : MonoBehaviour
{
    private static readonly Dictionary<Type, List<AudioTriggerZoneBase>> ActiveZonesByType =
        new Dictionary<Type, List<AudioTriggerZoneBase>>();

    [Header("Trigger Detection")]
    [Tooltip("Tag del jugador que activa esta zona.")]
    [SerializeField] protected string playerTag = "Player";

    [Tooltip("Si está activo, usa directamente el tag de este GameObject en lugar del selector del Inspector.")]
    [SerializeField] protected bool useObjectTagAsValue = false;

    [Tooltip("Si está activo, al salir sin quedar dentro de otra zona vuelve al valor de respaldo configurado.")]
    [SerializeField] protected bool revertOnExit = false;

    [Header("Overlap / Nesting")]
    [Tooltip("Prioridad cuando hay zonas superpuestas (mayor valor gana; a igual prioridad gana la última zona en la que entró el jugador).")]
    [SerializeField] private int zonePriority = 0;

    [Header("Optional Events")]
    [SerializeField] protected UnityEvent onPlayerEnter;
    [SerializeField] protected UnityEvent onPlayerExit;

    private Collider2D cachedCollider;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStaticState()
    {
        ActiveZonesByType.Clear();
    }

    protected virtual void Reset()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.isTrigger = true;
    }

    protected virtual void Awake()
    {
        cachedCollider = GetComponent<Collider2D>();
    }

    private void Start()
    {
        CheckInitialOverlap();
    }

    private void OnDisable()
    {
        RemoveFromActiveZones();
    }

    private void CheckInitialOverlap()
    {
        if (cachedCollider == null || !cachedCollider.isTrigger)
            return;

        Collider2D[] overlappingColliders = Physics2D.OverlapBoxAll(
            cachedCollider.bounds.center,
            cachedCollider.bounds.size,
            0f);

        for (int i = 0; i < overlappingColliders.Length; i++)
        {
            Collider2D other = overlappingColliders[i];
            if (other != null && other.CompareTag(playerTag))
            {
                EnterZone(other, isInitialCheck: true);
                break;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null || !other.CompareTag(playerTag))
            return;

        EnterZone(other, isInitialCheck: false);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other == null || !other.CompareTag(playerTag))
            return;

        RemoveFromActiveZones();
        onPlayerExit?.Invoke();

        AudioTriggerZoneBase fallbackZone = FindBestActiveZoneForPlayer(other);
        if (fallbackZone != null)
        {
            fallbackZone.ApplyCurrentZoneValue();
        }
        else if (revertOnExit)
        {
            ApplyValue(GetConfiguredExitValue());
        }
    }

    private void EnterZone(Collider2D playerCollider, bool isInitialCheck)
    {
        List<AudioTriggerZoneBase> activeList = GetOrCreateActiveList(GetType());
        activeList.Remove(this);
        activeList.Add(this);

        AudioTriggerZoneBase winner = FindBestActiveZoneForPlayer(playerCollider, isInitialCheck);
        if (winner == this)
        {
            ApplyCurrentZoneValue();
            onPlayerEnter?.Invoke();
        }
    }

    private void ApplyCurrentZoneValue()
    {
        string targetValue = useObjectTagAsValue ? gameObject.tag : GetConfiguredEnterValue();
        ApplyValue(targetValue);
    }

    private AudioTriggerZoneBase FindBestActiveZoneForPlayer(Collider2D playerCollider, bool preferSmallerBoundsOnTie = false)
    {
        if (!ActiveZonesByType.TryGetValue(GetType(), out List<AudioTriggerZoneBase> activeList) || activeList.Count == 0)
            return null;

        AudioTriggerZoneBase bestZone = null;

        // Recorremos de atrás hacia adelante para que, en empate de prioridad, gane la zona más reciente.
        for (int i = activeList.Count - 1; i >= 0; i--)
        {
            AudioTriggerZoneBase candidate = activeList[i];
            if (candidate == null || !candidate.isActiveAndEnabled || candidate.cachedCollider == null || !candidate.cachedCollider.enabled)
            {
                activeList.RemoveAt(i);
                continue;
            }

            if (playerCollider != null && !candidate.IsPlayerInsideZone(playerCollider))
            {
                activeList.RemoveAt(i);
                continue;
            }

            if (bestZone == null)
            {
                bestZone = candidate;
                continue;
            }

            if (candidate.zonePriority > bestZone.zonePriority)
            {
                bestZone = candidate;
            }
            else if (preferSmallerBoundsOnTie && candidate.zonePriority == bestZone.zonePriority)
            {
                if (candidate.GetBoundsArea() < bestZone.GetBoundsArea())
                    bestZone = candidate;
            }
        }

        return bestZone;
    }

    private bool IsPlayerInsideZone(Collider2D playerCollider)
    {
        if (cachedCollider == null || playerCollider == null)
            return false;

        return cachedCollider.IsTouching(playerCollider) || cachedCollider.bounds.Intersects(playerCollider.bounds);
    }

    private float GetBoundsArea()
    {
        if (cachedCollider == null)
            return float.MaxValue;

        Vector3 size = cachedCollider.bounds.size;
        return size.x * size.y;
    }

    private void RemoveFromActiveZones()
    {
        if (ActiveZonesByType.TryGetValue(GetType(), out List<AudioTriggerZoneBase> activeList))
            activeList.Remove(this);
    }

    private static List<AudioTriggerZoneBase> GetOrCreateActiveList(Type zoneType)
    {
        if (!ActiveZonesByType.TryGetValue(zoneType, out List<AudioTriggerZoneBase> list))
        {
            list = new List<AudioTriggerZoneBase>(4);
            ActiveZonesByType[zoneType] = list;
        }
        return list;
    }

    protected abstract string GetConfiguredEnterValue();
    protected abstract string GetConfiguredExitValue();
    protected abstract void ApplyValue(string value);
}
