using UnityEngine;

public class Box : MonoBehaviour
{
    [Header("Box Movement")]
    [SerializeField] private float obstacleCheckRadius = 0.12f;

    private Rigidbody2D rb;
    private BoxCollider2D boxCollider;
    private Vector2 targetPosition;
    private float moveSpeed;
    private bool isMoving;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }
        targetPosition = GetCurrentPosition2D();
    }

    public bool TryPush(Vector2 direction, float tileSize, float speed)
    {
        if (isMoving) return false;

        Vector2 dest = GetCurrentPosition2D() + direction * tileSize;
        if (IsBlocked(dest)) return false;

        targetPosition = dest;
        moveSpeed = Mathf.Max(0.01f, speed);
        isMoving = true;
        return true;
    }

    private void FixedUpdate()
    {
        if (!isMoving) return;

        Vector2 current = GetCurrentPosition2D();
        Vector2 next = Vector2.MoveTowards(current, targetPosition, moveSpeed * Time.fixedDeltaTime);

        if (rb != null)
            rb.MovePosition(next);
        else
            transform.position = new Vector3(next.x, next.y, transform.position.z);

        if ((targetPosition - next).sqrMagnitude <= 0.0001f)
        {
            if (rb != null) rb.MovePosition(targetPosition);
            else transform.position = new Vector3(targetPosition.x, targetPosition.y, transform.position.z);
            isMoving = false;
        }
    }

    private bool IsBlocked(Vector2 pos)
    {
        Vector2 size = GetColliderWorldSize();
        float angle = transform.eulerAngles.z;
        Vector2 center = pos + GetColliderWorldOffset();
        Collider2D[] hits = Physics2D.OverlapBoxAll(center, size, angle);
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i] != null && hits[i].gameObject != gameObject && hits[i].CompareTag("Wall"))
                return true;
        }
        return false;
    }

    private Vector2 GetCurrentPosition2D()
    {
        if (rb != null) return rb.position;
        return (Vector2)transform.position;
    }

    private Vector2 GetColliderWorldSize()
    {
        if (boxCollider == null) return new Vector2(obstacleCheckRadius * 2f, obstacleCheckRadius * 2f);
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
}
