using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Detecta el SurfaceType de la superficie bajo el jugador usando Physics2D.OverlapBox.
/// Mapea tags de colliders a SurfaceType a través de una lista configurable en el Inspector.
/// Colócalo en el mismo GameObject que PlayerMovement (o como hijo).
/// </summary>
public class SurfaceDetector : MonoBehaviour
{
    // ── Tipos ────────────────────────────────────────────────────────────────

    /// <summary>Par tag → SurfaceType configurable desde el Inspector.</summary>
    [Serializable]
    public struct SurfaceTagMapping
    {
        [Tooltip("Tag exacto del Collider2D de superficie (p.ej. 'Ground', 'Grass').")]
        public string tag;

        [Tooltip("SurfaceType que se devuelve cuando el collider tiene este tag.")]
        public SurfaceType surfaceType;
    }

    // ── Inspector ─────────────────────────────────────────────────────────────

    [Header("Surface Mappings")]
    [Tooltip("Lista de pares Tag → SurfaceType. El primero que coincida gana.")]
    [SerializeField] private List<SurfaceTagMapping> tagMappings = new List<SurfaceTagMapping>
    {
        new SurfaceTagMapping { tag = "Ground",  surfaceType = SurfaceType.Stone  },
        new SurfaceTagMapping { tag = "Grass",   surfaceType = SurfaceType.Grass  },
        new SurfaceTagMapping { tag = "Wood",    surfaceType = SurfaceType.Wood   },
        new SurfaceTagMapping { tag = "Sand",    surfaceType = SurfaceType.Sand   },
        new SurfaceTagMapping { tag = "Water",   surfaceType = SurfaceType.Water  },
        new SurfaceTagMapping { tag = "Metal",   surfaceType = SurfaceType.Metal  },
        new SurfaceTagMapping { tag = "Dirt",    surfaceType = SurfaceType.Dirt   },
    };

    [Header("Detection Settings")]
    [Tooltip("Tamaño de la caja de detección (en unidades del mundo). " +
             "Debe ser ligeramente más pequeña que el tile para evitar falsos positivos.")]
    [SerializeField] private Vector2 detectionSize = new Vector2(0.4f, 0.4f);

    [Tooltip("Offset local desde la posición del jugador hacia el centro de detección. " +
             "Útil si el origen del sprite no coincide con sus pies.")]
    [SerializeField] private Vector2 detectionOffset = Vector2.zero;

    [Tooltip("LayerMask con las capas que contienen tiles de superficie. " +
             "Mantén sólo las capas necesarias para evitar hits con el propio jugador.")]
    [SerializeField] private LayerMask surfaceLayerMask = ~0; // todo por defecto

    // ── Caché ─────────────────────────────────────────────────────────────────

    // ── API pública ───────────────────────────────────────────────────────────

    /// <summary>
    /// Realiza un OverlapBox en <paramref name="worldPos"/> + offset y devuelve
    /// el primer SurfaceType que coincida con los mappings.
    /// Retorna <see cref="SurfaceType.Default"/> si no encuentra nada.
    /// </summary>
    public SurfaceType Detect(Vector2 worldPos)
    {
        Vector2 center = worldPos + detectionOffset;

        Collider2D[] hits = Physics2D.OverlapBoxAll(
            center,
            detectionSize,
            0f,
            surfaceLayerMask);

        for (int i = 0; i < hits.Length; i++)
        {
            Collider2D hit = hits[i];
            if (hit == null) continue;

            // Ignorar el propio jugador y sus hijos
            if (hit.transform == transform || hit.transform.IsChildOf(transform))
                continue;

            SurfaceType found = GetSurfaceForTag(hit.tag);
            if (found != SurfaceType.Default)
                return found;
        }

        return SurfaceType.Default;
    }

    // ── Helpers privados ──────────────────────────────────────────────────────

    private SurfaceType GetSurfaceForTag(string tag)
    {
        for (int i = 0; i < tagMappings.Count; i++)
        {
            if (string.Equals(tagMappings[i].tag, tag, StringComparison.Ordinal))
                return tagMappings[i].surfaceType;
        }
        return SurfaceType.Default;
    }

    // ── Gizmos (sólo Editor) ──────────────────────────────────────────────────
#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0f, 1f, 0.5f, 0.4f);
        Vector2 center = (Vector2)transform.position + detectionOffset;
        Gizmos.DrawCube(center, detectionSize);
        Gizmos.color = new Color(0f, 1f, 0.5f, 1f);
        Gizmos.DrawWireCube(center, detectionSize);
    }
#endif
}
