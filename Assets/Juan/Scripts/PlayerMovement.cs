using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Tile Movement")]
    [SerializeField] private float tileSize = 1f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float wallCheckRadius = 0.1f;

    [Header("Layer Visuals")]
    [SerializeField] private GameObject capa1;
    [SerializeField] private GameObject capa2;
    [SerializeField] private GameObject capa3;
    [SerializeField] private GameObject capa4;

    [Header("Layer Mask Animation")]
    [SerializeField] private bool playMaskAnimationOnLayerChange = true;
    [SerializeField] private string maskTriggerName = "mask";
    [Tooltip("Tiempo a esperar antes de mostrar/ocultar la máscara. 0 = espera 1 frame.")]
    [SerializeField] private float maskVisualDelay = 0f;

    private Vector2 rawInput;
    private Vector2 lastRawInput;
    private Vector2 targetPosition;
    private bool isMoving;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    private float currentMoveSpeed;
    private Animator animator;

    private bool capa1Activa;
    private bool capa2Activa;
    private bool capa3Activa;
    private bool capa4Activa;

    private SpriteRenderer[] capa1Renderers;
    private SpriteRenderer[] capa2Renderers;
    private SpriteRenderer[] capa3Renderers;
    private SpriteRenderer[] capa4Renderers;

    private bool subscribedToLayers;

    private Coroutine layerVisualsRoutine;


    public bool canMove = true;

    public void DisableMovementForDeath()
    {
        canMove = false;
        isMoving = false;
        rawInput = Vector2.zero;
        lastRawInput = Vector2.zero;
        targetPosition = rb != null ? rb.position : (Vector2)transform.position;
        currentMoveSpeed = moveSpeed;

        SetChildrenSpriteRenderersEnabled(false);
    }

    public void DisableMovementForWin()
    {
        canMove = false;
        isMoving = false;
        rawInput = Vector2.zero;
        lastRawInput = Vector2.zero;
        targetPosition = rb != null ? rb.position : (Vector2)transform.position;
        currentMoveSpeed = moveSpeed;
    }

    public void EnableMovement()
    {
        canMove = true;
        rawInput = Vector2.zero;
        lastRawInput = Vector2.zero;

        SetChildrenSpriteRenderersEnabled(true);
        ApplyLayerVisuals();
    }


    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        animator = GetComponent<Animator>();


        boxCollider = GetComponent<BoxCollider2D>();

        rb.gravityScale = 0f;
        rb.freezeRotation = true;
        targetPosition = rb.position;
        currentMoveSpeed = moveSpeed;

        CacheLayerRenderers();
        ApplyLayerVisuals();
    }

    private void OnEnable()
    {
        TrySubscribeLayerEvents();
    }

    private void OnDisable()
    {
        UnsubscribeLayerEvents();
    }



    public void OnMove(InputAction.CallbackContext context)
    {
        rawInput = context.ReadValue<Vector2>();
    }

   

    private void Update()
    {
        if (!canMove || isMoving)
        {
            lastRawInput = rawInput;
            return;
        }

        bool newPress = rawInput.sqrMagnitude > 0.01f && lastRawInput.sqrMagnitude <= 0.01f;
        if (!newPress)
        {
            lastRawInput = rawInput;
            return;
        }

        Vector2 direction = GetStepDirection(rawInput);
        if (direction == Vector2.zero)
        {
            lastRawInput = rawInput;
            return;
        }

        Vector2 nextPosition = rb.position + direction * tileSize;
        if (IsWallAtPosition(nextPosition))
        {
            lastRawInput = rawInput;
            return;
        }

        // If there's a box at the next tile, try to push it.
        Box box = GetBoxAtPosition(nextPosition);
        if (box != null)
        {
            bool pushed = box.TryPush(direction, tileSize, moveSpeed * 0.5f);
            if (!pushed)
            {
                lastRawInput = rawInput;
                return;
            }
            currentMoveSpeed = moveSpeed * 0.5f;
        }
        else
        {
            currentMoveSpeed = moveSpeed;
        }

        targetPosition = nextPosition;
        isMoving = true;

        TriggerMoveAnimation();

        if (direction.x > 0) ApplyFlipX(true);
        else if (direction.x < 0) ApplyFlipX(false);

        lastRawInput = rawInput;
    }



    private void FixedUpdate()
    {
        if (!isMoving)
            return;

        Vector2 newPosition = Vector2.MoveTowards(rb.position, targetPosition, currentMoveSpeed * Time.fixedDeltaTime);
        rb.MovePosition(newPosition);

        if ((targetPosition - newPosition).sqrMagnitude <= 0.0001f)
        {
            rb.MovePosition(targetPosition);
            isMoving = false;
            currentMoveSpeed = moveSpeed;
        }
    }

    private Vector2 GetStepDirection(Vector2 input)
    {
        if (input.sqrMagnitude < 0.01f)
            return Vector2.zero;

        if (Mathf.Abs(input.x) > Mathf.Abs(input.y))
            return new Vector2(Mathf.Sign(input.x), 0f);

        return new Vector2(0f, Mathf.Sign(input.y));
    }

    private bool IsWallAtPosition(Vector2 position)
    {
        Vector2 size = GetColliderWorldSize();
        float angle = transform.eulerAngles.z;
        Vector2 center = position + GetColliderWorldOffset();
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, angle);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i] != null && hits[i].gameObject != gameObject && hits[i].CompareTag("Wall"))
                return true;
        }

        return false;
    }

    private Box GetBoxAtPosition(Vector2 position)
    {
        Vector2 size = GetColliderWorldSize();
        float angle = transform.eulerAngles.z;
        Vector2 center = position + GetColliderWorldOffset();
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, angle);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i] == null) continue;
            if (hits[i].gameObject == gameObject) continue;
            Box b = hits[i].GetComponent<Box>();
            if (b != null)
                return b;
        }
        return null;
    }

    private Vector2 GetColliderWorldSize()
    {
        if (boxCollider == null) return new Vector2(wallCheckRadius * 2f, wallCheckRadius * 2f);
        Vector2 s = boxCollider.size;
        Vector3 scale = transform.lossyScale;
        return new Vector2(s.x * Mathf.Abs(scale.x), s.y * Mathf.Abs(scale.y));
    }

    private Vector2 GetColliderWorldOffset()
    {
        if (boxCollider == null) return Vector2.zero;
        Vector2 local = boxCollider.offset;
        Vector3 world = transform.TransformVector(new Vector3(local.x, local.y, 0f));
        return new Vector2(world.x, world.y);
    }

    private void ApplyFlipX(bool flip)
    {
        if (spriteRenderer != null)
            spriteRenderer.flipX = flip;

        SpriteRenderer[] children = GetComponentsInChildren<SpriteRenderer>();
        for (int i = 0; i < children.Length; i++)
        {
            SpriteRenderer sr = children[i];
            if (sr == null || sr == spriteRenderer) continue;
            sr.flipX = flip;
        }
    }

    private void CacheLayerRenderers()
    {
        capa1Renderers = capa1 != null ? capa1.GetComponentsInChildren<SpriteRenderer>(true) : null;
        capa2Renderers = capa2 != null ? capa2.GetComponentsInChildren<SpriteRenderer>(true) : null;
        capa3Renderers = capa3 != null ? capa3.GetComponentsInChildren<SpriteRenderer>(true) : null;
        capa4Renderers = capa4 != null ? capa4.GetComponentsInChildren<SpriteRenderer>(true) : null;
    }

    private void ApplyLayerVisuals()
    {
        int highest = GetHighestActiveLayer();
        SetRenderersEnabled(capa1Renderers, highest == 1);
        SetRenderersEnabled(capa2Renderers, highest == 2);
        SetRenderersEnabled(capa3Renderers, highest == 3);
        SetRenderersEnabled(capa4Renderers, highest == 4);
    }

    private int GetHighestActiveLayer()
    {
        if (capa4Activa) return 4;
        if (capa3Activa) return 3;
        if (capa2Activa) return 2;
        if (capa1Activa) return 1;
        return 0;
    }

    private void SetRenderersEnabled(SpriteRenderer[] renderers, bool enabled)
    {
        if (renderers == null) return;
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;
            renderers[i].enabled = enabled;
        }
    }

    private void TrySubscribeLayerEvents()
    {
        if (subscribedToLayers) return;
        if (EventManager.Instance == null) return;

        EventManager.Instance.EnActivarCapa += OnLayerActivated;
        EventManager.Instance.EnDesactivarCapa += OnLayerDeactivated;
        subscribedToLayers = true;
    }

    private void UnsubscribeLayerEvents()
    {
        if (!subscribedToLayers) return;
        if (EventManager.Instance == null) return;

        EventManager.Instance.EnActivarCapa -= OnLayerActivated;
        EventManager.Instance.EnDesactivarCapa -= OnLayerDeactivated;
        subscribedToLayers = false;
    }

    private void OnLayerActivated(int capa)
    {
        switch (capa)
        {
            case 1: capa1Activa = true; break;
            case 2: capa2Activa = true; break;
            case 3: capa3Activa = true; break;
            case 4: capa4Activa = true; break;
        }
        QueueLayerVisualsUpdate();
    }

    private void OnLayerDeactivated(int capa)
    {
        switch (capa)
        {
            case 1: capa1Activa = false; break;
            case 2: capa2Activa = false; break;
            case 3: capa3Activa = false; break;
            case 4: capa4Activa = false; break;
        }
        QueueLayerVisualsUpdate();
    }

    private void QueueLayerVisualsUpdate()
    {
        if (layerVisualsRoutine != null)
        {
            StopCoroutine(layerVisualsRoutine);
            layerVisualsRoutine = null;
        }

        if (!playMaskAnimationOnLayerChange || string.IsNullOrWhiteSpace(maskTriggerName))
        {
            ApplyLayerVisuals();
            return;
        }

        bool hasAnyAnimator = animator != null || GetComponentsInChildren<Animator>(true).Any(a => a != null);
        if (!hasAnyAnimator)
        {
            ApplyLayerVisuals();
            return;
        }

        TriggerMaskAnimation();
        layerVisualsRoutine = StartCoroutine(ApplyLayerVisualsAfterMask());
    }

    private IEnumerator ApplyLayerVisualsAfterMask()
    {
        if (maskVisualDelay <= 0f)
            yield return null;
        else
            yield return new WaitForSeconds(maskVisualDelay);

        ApplyLayerVisuals();
        layerVisualsRoutine = null;
    }

    private void TriggerMoveAnimation()
    {
        if (animator != null)
            animator.SetTrigger("move");

        Animator[] childAnimators = GetComponentsInChildren<Animator>();
        for (int i = 0; i < childAnimators.Length; i++)
        {
            Animator a = childAnimators[i];
            if (a == null || a == animator) continue;
            a.SetTrigger("move");
        }
    }

    private void TriggerMaskAnimation()
    {
        if (animator != null)
            animator.SetTrigger(maskTriggerName);
    }

    private void SetChildrenSpriteRenderersEnabled(bool enabled)
    {
        SpriteRenderer[] children = GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < children.Length; i++)
        {
            SpriteRenderer sr = children[i];
            if (sr == null || sr == spriteRenderer) continue;
            sr.enabled = enabled;
        }
    }
}
