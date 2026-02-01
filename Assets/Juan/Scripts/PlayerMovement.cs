using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public class PlayerMovement : MonoBehaviour
{
    [Header("Tile Movement")]
    [SerializeField] private float tileSize = 1f;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float wallCheckRadius = 0.1f;

    private Vector2 rawInput;
    private Vector2 lastRawInput;
    private Vector2 targetPosition;
    private bool isMoving;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    private float currentMoveSpeed;
    private Animator animator;


    public bool canMove = true;

    public void DisableMovementForDeath()
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

        if (animator != null)
            animator.SetTrigger("move");

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
}
