using UnityEngine;

public class Box : MonoBehaviour
{
    [Header("Box Movement")]
    [SerializeField] private float obstacleCheckRadius = 0.12f;

    private Rigidbody2D rb;
    private Vector2 targetPosition;
    private float moveSpeed;
    private bool isMoving;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
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
        Collider2D[] hits = Physics2D.OverlapCircleAll(pos, obstacleCheckRadius);
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
}
