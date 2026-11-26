using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Detector acoplable al GameObject del Player para actualizar Location y Surface_type
/// leyendo directamente el tag de cualquier Collider2D Trigger que toque en OnTriggerEnter2D.
/// Ideal si los objetos del mapa ya tienen asignados los tags (Normal, Tienda, PreBoss, Dirt, Water, Concrete, Grass).
/// </summary>
public class PlayerAudioTriggerDetector : MonoBehaviour
{
    [Header("Player Verification")]
    [SerializeField] private string playerTag = "Player";

    [Header("Location Tags -> FMOD 'Location'")]
    [SerializeField] private List<string> locationTags = new List<string>
    {
        "Normal",
        "Tienda",
        "PreBoss"
    };

    [Header("Surface Tags -> FMOD 'Surface_type'")]
    [SerializeField] private List<string> surfaceTags = new List<string>
    {
        "Dirt",
        "Water",
        "Concrete",
        "Grass"
    };

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == null || AudioManager.instance == null)
            return;

        // Validar que este componente pertenece al jugador cuando usa tag
        if (!string.IsNullOrEmpty(playerTag) && !CompareTag(playerTag))
            return;

        string otherTag = other.tag;
        if (string.IsNullOrEmpty(otherTag) || string.Equals(otherTag, "Untagged", StringComparison.Ordinal))
            return;

        if (ContainsTag(locationTags, otherTag))
        {
            AudioManager.instance.SetLocation(otherTag);
        }
        else if (ContainsTag(surfaceTags, otherTag))
        {
            AudioManager.instance.SetSurfaceType(otherTag);
        }
    }

    private static bool ContainsTag(List<string> list, string tagToCheck)
    {
        if (list == null)
            return false;

        for (int i = 0; i < list.Count; i++)
        {
            if (string.Equals(list[i], tagToCheck, StringComparison.Ordinal))
                return true;
        }
        return false;
    }
}
