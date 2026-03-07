using System.Collections;
using UnityEngine;

public class DamageWall : MonoBehaviour
{
    [SerializeField] private float activeDuration = 30f;
    private const float MovingThresholdSqr = 0.0001f;

    private Rigidbody2D rb;
    private Coroutine lifeCoroutine;

    public bool IsMoving
    {
        get
        {
            if (rb == null)
                rb = GetComponent<Rigidbody2D>();
            if (rb == null)
                return false;
            return rb.linearVelocity.sqrMagnitude > MovingThresholdSqr;
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Launch(Vector2 direction, float force)
    {
        // Reset velocity
        rb.linearVelocity = Vector2.zero;

        // Apply movement
        rb.linearVelocity = direction * force;

        // Restart timer if needed
        if (lifeCoroutine != null)
            StopCoroutine(lifeCoroutine);

        lifeCoroutine = StartCoroutine(LifeTimer());
    }

    private IEnumerator LifeTimer()
    {
        yield return new WaitForSeconds(activeDuration);

        rb.linearVelocity = Vector2.zero;
    }
}