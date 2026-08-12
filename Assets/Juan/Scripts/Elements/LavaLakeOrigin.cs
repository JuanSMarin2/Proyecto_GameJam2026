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
    [SerializeField] private bool useTiledSizing = true; // Use SpriteRenderer.size when drawMode is Tiled
    [SerializeField] private bool syncColliderSize = false; // Optionally sync BoxCollider2D to rendered size

    [Header("Sprites")]
    [SerializeField] private Sprite growingSprite; // Sprite used while lava length is changing
    [SerializeField] private Sprite idleSprite;    // Sprite used when lava is static
    [SerializeField] private float stateEpsilon = 0.001f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip blockedByBoxClip;
    [Range(0f, 1f)]
    [SerializeField] private float blockedByBoxVolume = 1f;

    private float currentLength = 0f;
    private bool wasBlockedByBox = false;

    private void Start()
    {
        currentLength = startAtZero ? 0f : maxLength;

        // Fallback opcional
        if (audioSource == null) audioSource = GetComponent<AudioSource>();

        if (Application.isPlaying && buildOnStart)
            BuildLake();
    }

    private void Update()
    {
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
        Collider2D closestCol = null;

        for (int i = 0; i < hits.Length; i++)
        {
            var col = hits[i].collider;
            if (col == null) continue;

            if (IsBlocking(col))
            {
                float d = hits[i].distance;
                if (d < closest)
                {
                    closest = d;
                    closestCol = col;
                }
            }
        }

        if (closest != float.MaxValue)
            targetLength = Mathf.Max(0f, closest - contactMargin);

        // ===== SONIDO: cuando se bloquea por Box y comprime (solo 1 vez por bloqueo) =====
        bool blockedByBox = (closestCol != null && closestCol.CompareTag("Box"));
        bool isCompressing = targetLength < (currentLength - stateEpsilon);

        if (blockedByBox && isCompressing && !wasBlockedByBox)
        {
           

            // Si prefieres tu sistema:
            SoundManager.PlaySound(SoundType.LavaMoving);
            Debug.Log("Lava Lake: blocked by Box, playing sound."+ SoundType.LavaMoving);
        }

        // Importante: solo consideramos "bloqueado por box" mientras el bloqueo actual sea por box.
        wasBlockedByBox = blockedByBox;
        // ==============================================================================

        float speed = targetLength > currentLength ? expandSpeed : contractSpeed;
        currentLength = Mathf.MoveTowards(currentLength, targetLength, speed * Time.deltaTime);

        // Position lava lake centered along the cast
        Vector3 center = (Vector3)(origin + dir * (currentLength * 0.5f));
        center.z = lavaLake.position.z;
        lavaLake.position = center;

        // Stretch along the cast axis, set perpendicular thickness
        SpriteRenderer sr = lavaLake.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            bool isGrowing = Mathf.Abs(targetLength - currentLength) > stateEpsilon;
            Sprite desired = isGrowing ? growingSprite : idleSprite;
            if (desired != null && sr.sprite != desired)
                sr.sprite = desired;
        }

        bool canTile = useTiledSizing && sr != null && sr.drawMode == SpriteDrawMode.Tiled;

        if (canTile)
        {
            Vector3 ls = lavaLake.localScale;
            float sx = Mathf.Max(0.0001f, Mathf.Abs(ls.x));
            float sy = Mathf.Max(0.0001f, Mathf.Abs(ls.y));

            float len = Mathf.Max(0.001f, currentLength);
            float thick = Mathf.Max(0.001f, thickness);

            if (horizontal)
                sr.size = new Vector2(len / sx, thick / sy);
            else
                sr.size = new Vector2(thick / sx, len / sy);

            sr.tileMode = SpriteTileMode.Continuous;

            if (syncColliderSize)
            {
                BoxCollider2D bc = lavaLake.GetComponent<BoxCollider2D>();
                if (bc != null)
                    bc.size = sr.size;
            }
        }
        else
        {
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