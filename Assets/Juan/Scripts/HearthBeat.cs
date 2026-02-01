using UnityEngine;

public class HearthBeat : MonoBehaviour
{
    [Header("Beat Settings")]
    [Tooltip("Latidos por segundo.")]
    [SerializeField] private float beatSpeed = 2f;

    [Tooltip("Fuerza del palpiteo como porcentaje de escala (0.1 = 10%).")]
    [SerializeField] private float beatStrength = 0.1f;

    [Tooltip("Si está activo, no se ve afectado por Time.timeScale.")]
    [SerializeField] private bool useUnscaledTime;

    private Vector3 baseScale;

    private void Awake()
    {
        baseScale = transform.localScale;
    }

    private void OnEnable()
    {
        baseScale = transform.localScale;
    }

    private void Update()
    {
        float speed = Mathf.Max(0f, beatSpeed);
        float strength = Mathf.Max(0f, beatStrength);

        float time = useUnscaledTime ? Time.unscaledTime : Time.time;

        // 0..1
        float pulse01 = (Mathf.Sin(time * speed * Mathf.PI * 2f) + 1f) * 0.5f;
        float scaleFactor = 1f + strength * pulse01;

        transform.localScale = baseScale * scaleFactor;
    }
}
