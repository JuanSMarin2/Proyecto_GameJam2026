using UnityEngine;

public class GridCollision : MonoBehaviour
{
    [SerializeField] private float wallCheckRadius = 0.1f;

    private BoxCollider2D boxCollider;

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
    }

    public bool IsWallAtPosition(Vector2 position)
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

    public Box GetBoxAtPosition(Vector2 position)
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
}