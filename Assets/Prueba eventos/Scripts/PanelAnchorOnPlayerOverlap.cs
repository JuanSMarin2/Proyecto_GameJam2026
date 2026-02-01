using UnityEngine;

public class PanelAnchorOnPlayerOverlap : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform panelDetector;
    [SerializeField] private RectTransform panelObjetivo;

    [Header("Player")]
    [SerializeField] private Transform player;
    [SerializeField] private string playerTag = "Player";

    [Header("Cameras / Canvas")]
    [Tooltip("Camera usada para convertir el Player (mundo) a ScreenPoint. Si está vacío, usa Camera.main")]
    [SerializeField] private Camera worldCamera;
    [Tooltip("Canvas donde vive el panelDetector. Si está vacío, se intenta inferir desde el panelDetector")]
    [SerializeField] private Canvas canvas;

    [Header("Offsets (optional)")]
    [SerializeField] private Vector2 offsetArribaDerecha = Vector2.zero;
    [SerializeField] private Vector2 offsetAbajoDerecha = Vector2.zero;

    private bool isOverlapping;

    private void Awake()
    {
        if (canvas == null && panelDetector != null)
            canvas = panelDetector.GetComponentInParent<Canvas>();

        if (worldCamera == null)
            worldCamera = Camera.main;

        if (player == null && !string.IsNullOrWhiteSpace(playerTag))
        {
            GameObject found = GameObject.FindGameObjectWithTag(playerTag);
            if (found != null) player = found.transform;
        }
    }

    private void OnEnable()
    {
        // Estado por defecto
        SetAbajoDerecha();
        isOverlapping = false;
    }

    private void Update()
    {
        if (panelDetector == null || panelObjetivo == null) return;
        if (player == null) return;
        if (worldCamera == null) return;

        Vector3 screenPos = worldCamera.WorldToScreenPoint(player.position);
        if (screenPos.z < 0f)
        {
            // Player behind camera
            if (isOverlapping)
            {
                isOverlapping = false;
                SetAbajoDerecha();
            }
            return;
        }

        Camera uiCamera = GetUICamera();
        bool nowOverlapping = RectTransformUtility.RectangleContainsScreenPoint(panelDetector, screenPos, uiCamera);
        if (nowOverlapping == isOverlapping) return;

        isOverlapping = nowOverlapping;
        if (isOverlapping) SetArribaDerecha();
        else SetAbajoDerecha();
    }

    private void SetArribaDerecha()
    {
        if (panelObjetivo == null) return;

        panelObjetivo.anchorMin = new Vector2(1f, 1f);
        panelObjetivo.anchorMax = new Vector2(1f, 1f);
        panelObjetivo.pivot = new Vector2(1f, 1f);
        panelObjetivo.anchoredPosition = offsetArribaDerecha;
    }

    private void SetAbajoDerecha()
    {
        if (panelObjetivo == null) return;

        panelObjetivo.anchorMin = new Vector2(1f, 0f);
        panelObjetivo.anchorMax = new Vector2(1f, 0f);
        panelObjetivo.pivot = new Vector2(1f, 0f);
        panelObjetivo.anchoredPosition = offsetAbajoDerecha;
    }

    private Camera GetUICamera()
    {
        // Para Screen Space Overlay, RectTransformUtility espera camera = null
        if (canvas == null) return null;

        switch (canvas.renderMode)
        {
            case RenderMode.ScreenSpaceOverlay:
                return null;
            case RenderMode.ScreenSpaceCamera:
            case RenderMode.WorldSpace:
                return canvas.worldCamera != null ? canvas.worldCamera : worldCamera;
            default:
                return null;
        }
    }

    private void OnValidate()
    {
        if (canvas == null && panelDetector != null)
            canvas = panelDetector.GetComponentInParent<Canvas>();

        if (worldCamera == null)
            worldCamera = Camera.main;
    }
}
