using System;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class WaterVisual : MonoBehaviour
{
    [Header("Appearance")]
    [SerializeField] private Color bodyColor = new Color(0.22f, 0.56f, 0.95f, 0.88f);
    [SerializeField] private Color headColor = new Color(0.42f, 0.80f, 1f, 0.98f);
    [SerializeField] private float bodyScale = 1f;
    [SerializeField] private float headScale = 1.05f;

    [Header("Motion")]
    [SerializeField] private float moveDuration = 0.15f;
    [SerializeField] private float removalDuration = 0.15f;
    [SerializeField] private float flowWaveAmplitude = 0.03f;
    [SerializeField] private float flowWaveSpeed = 6f;
    [SerializeField] private float flowPulseSpeed = 7.5f;

    private SpriteRenderer spriteRenderer;
    private Vector3 moveStartPosition;
    private Vector3 moveTargetPosition;
    private Vector3 restingPosition;
    private float moveTimer;
    private bool isMoving;
    private bool isHead;
    private bool isRemoving;
    private bool isFlowing;
    private float removalTimer;
    private Vector3 baseScale;
    private Action removalCallback;
    private Vector2Int flowDirection = Vector2Int.down;
    private Quaternion baseRotation = Quaternion.identity;
    private SpriteRenderer accentRenderer;
    private Transform accentTransform;
    private Vector3 accentBaseLocalPosition;
    private Color accentBaseColor;

    public Vector2Int Cell { get; private set; }

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        baseScale = transform.localScale;
        baseRotation = transform.localRotation;
        restingPosition = transform.position;

        if (spriteRenderer.sprite == null)
            spriteRenderer.sprite = CreateFallbackSprite();

        spriteRenderer.color = bodyColor;
        CreateAccentVisual();
    }

    private void Update()
    {
        if (isMoving)
            UpdateMovement();

        if (isRemoving)
            UpdateRemoval();

        UpdateFlowMotion();
    }

    public void Initialize(Vector2Int cell, Vector3 startWorld, Vector3 targetWorld, bool animate, bool headState)
    {
        Cell = cell;
        isHead = headState;
        isRemoving = false;
        removalCallback = null;
        isFlowing = animate;
        flowDirection = Vector2Int.down;
        restingPosition = targetWorld;

        ApplyAppearance();

        if (animate)
        {
            moveStartPosition = startWorld;
            moveTargetPosition = targetWorld;
            moveTimer = 0f;
            isMoving = true;
            transform.position = moveStartPosition;
        }
        else
        {
            isMoving = false;
            transform.position = targetWorld;
            restingPosition = targetWorld;
        }
    }

    public void SetCell(Vector2Int cell, Vector3 targetWorld, bool animate)
    {
        Cell = cell;

        if (animate)
        {
            moveStartPosition = transform.position;
            moveTargetPosition = targetWorld;
            moveTimer = 0f;
            isMoving = true;
            return;
        }

        isMoving = false;
        transform.position = targetWorld;
        restingPosition = targetWorld;
    }

    public void SetHead(bool headState)
    {
        isHead = headState;
        ApplyAppearance();
    }

    public void SetFlowDirection(Vector2Int direction)
    {
        flowDirection = direction;
        ApplyAppearance();
    }

    public void SetFlowActive(bool flowActive)
    {
        isFlowing = flowActive;
    }

    public void CancelRemoval()
    {
        if (!isRemoving)
            return;

        isRemoving = false;
        removalTimer = 0f;
        removalCallback = null;
        transform.localScale = baseScale * (isHead ? headScale : bodyScale);
        spriteRenderer.color = isHead ? headColor : bodyColor;
        if (accentRenderer != null)
            accentRenderer.color = accentBaseColor;
    }

    public void BeginRemoval(Action onRemoved)
    {
        isRemoving = true;
        removalTimer = 0f;
        removalCallback = onRemoved;
    }

    private void UpdateMovement()
    {
        moveTimer += Time.deltaTime;
        float t = moveDuration <= 0.0001f ? 1f : Mathf.Clamp01(moveTimer / moveDuration);
        transform.position = Vector3.Lerp(moveStartPosition, moveTargetPosition, t);

        if (t >= 1f)
        {
            transform.position = moveTargetPosition;
            restingPosition = moveTargetPosition;
            isMoving = false;
        }
    }

    private void UpdateFlowMotion()
    {
        if (!isFlowing || isRemoving || isMoving)
        {
            transform.localRotation = baseRotation;
            if (accentTransform != null)
                accentTransform.localPosition = accentBaseLocalPosition;
            return;
        }

        Vector3 directionVector = GetDirectionVector(flowDirection);
        float wave = Mathf.Sin(Time.time * flowWaveSpeed) * flowWaveAmplitude;
        transform.position = restingPosition;

        float angle = GetDirectionAngle(flowDirection);
        transform.localRotation = baseRotation * Quaternion.Euler(0f, 0f, angle);

        if (accentTransform != null)
        {
            Vector3 pulseOffset = directionVector * wave;
            pulseOffset += new Vector3(-directionVector.y, directionVector.x, 0f) * (Mathf.Sin(Time.time * (flowPulseSpeed * 0.6f)) * flowWaveAmplitude * 0.5f);
            accentTransform.localPosition = accentBaseLocalPosition + pulseOffset;

            float pulse = 0.85f + Mathf.Sin(Time.time * flowPulseSpeed) * 0.08f;
            Color accentColor = accentBaseColor;
            if (isHead)
                accentColor = Color.Lerp(accentColor, headColor, 0.25f);
            accentColor.a *= pulse;
            accentRenderer.color = accentColor;
        }

        Color baseColor = isHead ? headColor : bodyColor;
        baseColor.a *= 0.96f + Mathf.Sin(Time.time * flowPulseSpeed) * 0.02f;
        spriteRenderer.color = baseColor;
    }

    private void UpdateRemoval()
    {
        removalTimer += Time.deltaTime;
        float t = removalDuration <= 0.0001f ? 1f : Mathf.Clamp01(removalTimer / removalDuration);
        float remaining = 1f - t;
        transform.localScale = baseScale * remaining;

        Color color = isHead ? headColor : bodyColor;
        color.a *= remaining;
        spriteRenderer.color = color;

        if (t >= 1f)
        {
            isRemoving = false;
            removalCallback?.Invoke();
            Destroy(gameObject);
        }
    }

    private void ApplyAppearance()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        transform.localScale = baseScale * (isHead ? headScale : bodyScale);
        spriteRenderer.color = isHead ? headColor : bodyColor;
        transform.localRotation = baseRotation * Quaternion.Euler(0f, 0f, GetDirectionAngle(flowDirection));

        if (accentRenderer != null)
        {
            accentRenderer.color = accentBaseColor;
            accentTransform.localPosition = accentBaseLocalPosition;
        }
    }

    private static Vector3 GetDirectionVector(Vector2Int direction)
    {
        if (direction == Vector2Int.right)
            return Vector3.right;

        if (direction == Vector2Int.left)
            return Vector3.left;

        if (direction == Vector2Int.up)
            return Vector3.up;

        if (direction == Vector2Int.down)
            return Vector3.down;

        return Vector3.zero;
    }

    private static float GetDirectionAngle(Vector2Int direction)
    {
        if (direction == Vector2Int.right)
            return 0f;

        if (direction == Vector2Int.up)
            return 90f;

        if (direction == Vector2Int.left)
            return 180f;

        if (direction == Vector2Int.down)
            return -90f;

        return 0f;
    }

    private void CreateAccentVisual()
    {
        GameObject accentObject = new GameObject("WaterAccent");
        accentObject.transform.SetParent(transform, false);
        accentTransform = accentObject.transform;
        accentBaseLocalPosition = Vector3.zero;

        accentRenderer = accentObject.AddComponent<SpriteRenderer>();
        accentRenderer.sprite = spriteRenderer.sprite;
        accentRenderer.sortingLayerID = spriteRenderer.sortingLayerID;
        accentRenderer.sortingOrder = spriteRenderer.sortingOrder + 1;

        accentBaseColor = new Color(1f, 1f, 1f, 0.35f);
        accentRenderer.color = accentBaseColor;
        accentTransform.localScale = Vector3.one * 0.35f;
    }

    private static Sprite CreateFallbackSprite()
    {
        return Sprite.Create(Texture2D.whiteTexture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
    }
}