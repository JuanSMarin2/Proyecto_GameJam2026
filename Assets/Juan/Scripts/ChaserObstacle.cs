using UnityEngine;
using System.Collections.Generic;

public class ChaserObstacle : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Camera cam;

    [Header("Chase Settings")]
    [SerializeField] private float delaySeconds = 0.5f;
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float stopDistance = 0.01f;
    [SerializeField] private float historyBufferSeconds = 2f;
    [SerializeField] private float viewportPadding = 0f;

    private readonly Queue<Sample> history = new Queue<Sample>();
    private Vector2 targetPos;
    private Rigidbody2D rb;
    private float currentSpeed;

    private struct Sample
    {
        public float t;
        public Vector2 p;
        public Sample(float time, Vector2 pos)
        {
            t = time;
            p = pos;
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        if (player == null)
        {
            GameObject found = GameObject.FindGameObjectWithTag("Player");
            if (found != null) player = found.transform;
        }

        if (cam == null) cam = Camera.main;

        targetPos = GetCurrentPosition2D();
        currentSpeed = moveSpeed;
    }

    private void Update()
    {
        // Collect player's position history
        if (player != null)
        {
            history.Enqueue(new Sample(Time.time, (Vector2)player.position));

            // Trim old samples beyond buffer window
            float cutoff = Time.time - Mathf.Max(historyBufferSeconds, delaySeconds + 0.1f);
            while (history.Count > 0 && history.Peek().t < cutoff)
                history.Dequeue();
        }

        // Determine target position from history with delay
        float desiredTime = Time.time - delaySeconds;
        Vector2 candidate = targetPos;
        if (history.Count > 0)
        {
            Sample[] samples = history.ToArray();

            // Find the last sample <= desiredTime (or earliest if none)
            int idx = -1;
            for (int i = 0; i < samples.Length; i++)
            {
                if (samples[i].t <= desiredTime) idx = i; else break;
            }

            if (idx >= 0)
            {
                candidate = samples[idx].p;
            }
            else
            {
                // Not enough history yet; go to the earliest known
                candidate = samples[0].p;
            }
        }

        targetPos = candidate;

        // Update current speed based on on-screen visibility
        currentSpeed = IsOnScreen() ? moveSpeed : 0f;
    }

    private void FixedUpdate()
    {
        Vector2 current = GetCurrentPosition2D();
        if ((targetPos - current).sqrMagnitude <= stopDistance * stopDistance)
            return;

        Vector2 next = Vector2.MoveTowards(current, targetPos, currentSpeed * Time.fixedDeltaTime);

        if (rb != null)
            rb.MovePosition(next);
        else
            transform.position = new Vector3(next.x, next.y, transform.position.z);
    }

    private Vector2 GetCurrentPosition2D()
    {
        if (rb != null) return rb.position;
        return (Vector2)transform.position;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.4f, 0.2f, 0.8f);
        Gizmos.DrawWireSphere(new Vector3(targetPos.x, targetPos.y, transform.position.z), 0.15f);
    }

    private bool IsOnScreen()
    {
        if (cam == null) return true; // If no camera set, don't block movement
        Vector3 vp = cam.WorldToViewportPoint(transform.position);
        float pad = viewportPadding;
        return vp.z > 0f && vp.x >= 0f - pad && vp.x <= 1f + pad && vp.y >= 0f - pad && vp.y <= 1f + pad;
    }
}
