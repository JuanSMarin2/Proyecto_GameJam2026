using UnityEngine;

public class LavaLakeOrigin : MonoBehaviour
{
    [Header("Lava Lake Setup")]
    [SerializeField] private Transform lavaLake; // Serialized object to stretch
    [SerializeField] private bool horizontal = true;
    [SerializeField] private bool positive = true;
    [SerializeField] private float maxLength = 50f;
    [SerializeField] private float thickness = 0.2f;
    [SerializeField] private float contactMargin = 0.01f;
    [SerializeField] private bool buildOnStart = true;
    [SerializeField] private float expandSpeed = 10f;
    [SerializeField] private float contractSpeed = 15f;
    [SerializeField] private bool startAtZero = true;

    private float currentLength = 0f;

    private void Start()
    {
        currentLength = startAtZero ? 0f : 0f;
        if (Application.isPlaying && buildOnStart)
            BuildLake();
    }

    private void Update()
    {
        // Live updates only during play so Box/Wall changes affect lava instantly
        if (Application.isPlaying)
            BuildLake();
    }

    public void BuildLake()
    {
        if (lavaLake == null) return;

        Vector2 origin = transform.position;
        Vector2 dir = GetDirection();

        float targetLength = maxLength;
        RaycastHit2D[] hits = Physics2D.RaycastAll(origin, dir, maxLength);
        float closest = float.MaxValue;
        for (int i = 0; i < hits.Length; i++)
        {
            if (hits[i].collider == null) continue;
            if (IsBlocking(hits[i].collider))
            {
                float d = hits[i].distance;
                if (d < closest)
                    closest = d;
            }
        }

        if (closest != float.MaxValue)
            targetLength = Mathf.Max(0f, closest - contactMargin);

        float speed = targetLength > currentLength ? expandSpeed : contractSpeed;
        currentLength = Mathf.MoveTowards(currentLength, targetLength, speed * Time.deltaTime);

        // Position lava lake centered along the cast
        Vector3 center = (Vector3)(origin + dir * (currentLength * 0.5f));
        center.z = lavaLake.position.z;
        lavaLake.position = center;

        // Stretch along the cast axis, set perpendicular thickness
        Vector3 scale = lavaLake.localScale;
        if (horizontal)
        {
            scale.x = Mathf.Max(0f, currentLength);
            scale.y = Mathf.Max(0.001f, thickness);
        }
        else
        {
            scale.y = Mathf.Max(0f, currentLength);
            scale.x = Mathf.Max(0.001f, thickness);
        }
        lavaLake.localScale = scale;
    }

    private Vector2 GetDirection()
    {
        if (horizontal)
            return positive ? Vector2.right : Vector2.left;
        else
            return positive ? Vector2.up : Vector2.down;
    }

    private bool IsBlocking(Collider2D col)
    {
        if (col == null) return false;
        return col.CompareTag("Wall") || col.CompareTag("Box");
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 origin = transform.position;
        Vector2 dir = GetDirection();
        Gizmos.color = new Color(1f, 0.4f, 0f, 0.8f);
        Gizmos.DrawLine(origin, origin + dir * maxLength);
        Gizmos.DrawWireSphere(origin, 0.05f);
    }
}
