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
    [SerializeField] private Camera worldCamera;
    [SerializeField] private Canvas canvas;

    [Header("Y Offset When Overlapping")]
    [SerializeField] private float overlapY;

    private bool isOverlapping;
    private Vector2 originalPosition;

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

        if (panelObjetivo != null)
            originalPosition = panelObjetivo.anchoredPosition;
    }

    private void OnEnable()
    {
        isOverlapping = false;

        if (panelObjetivo != null)
            panelObjetivo.anchoredPosition = originalPosition;
    }

    private void Update()
    {
        if (panelDetector == null || panelObjetivo == null) return;
        if (player == null) return;
        if (worldCamera == null) return;

        Vector3 screenPos = worldCamera.WorldToScreenPoint(player.position);

        if (screenPos.z < 0f)
        {
            if (isOverlapping)
            {
                isOverlapping = false;
                ResetPosition();
            }
            return;
        }

        Camera uiCamera = GetUICamera();

        bool nowOverlapping = RectTransformUtility.RectangleContainsScreenPoint(
            panelDetector,
            screenPos,
            uiCamera
        );

        if (nowOverlapping == isOverlapping) return;

        isOverlapping = nowOverlapping;

        if (isOverlapping)
            MoveUp();
        else
            ResetPosition();
    }

    private void MoveUp()
    {
        Vector2 pos = panelObjetivo.anchoredPosition;
        pos.y = overlapY;
        panelObjetivo.anchoredPosition = pos;
    }

    private void ResetPosition()
    {
        panelObjetivo.anchoredPosition = originalPosition;
    }

    private Camera GetUICamera()
    {
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