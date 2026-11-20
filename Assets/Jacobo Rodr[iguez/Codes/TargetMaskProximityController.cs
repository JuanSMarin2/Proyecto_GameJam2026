using FMOD.Studio;
using FMODUnity;
using UnityEngine;

public class TargetMaskProximityController : MonoBehaviour
{
    [Header("FMOD")]
    [SerializeField] private EventReference proximityEvent;
    [SerializeField] private string proximityParameterName = "Target_Proximity";
    [SerializeField] private bool use3DPositioning = false;

    [Header("Targets & Detection")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform targetMaskTransform;
    [SerializeField, Min(0.1f)] private float maxDetectionRadius = 15f;
    [SerializeField, Min(0.01f)] private float minDistance = 0.5f;

    [Header("Response Shaping")]
    [Tooltip("Permite acelerar o suavizar cómo crece la proximidad (X = lineal 0..1, Y = salida 0..1).")]
    [SerializeField] private AnimationCurve proximityCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    [SerializeField, Min(0f)] private float parameterSmoothSpeed = 8f;

    [Header("Gizmos Debug")]
    [Tooltip("Muestra los radios de proximidad en la vista de Escena.")]
    [SerializeField] private bool showGizmos = true;
    [Tooltip("Si está activo, sólo dibuja los Gizmos al seleccionar el GameObject.")]
    [SerializeField] private bool drawOnlyWhenSelected = true;
    [SerializeField] private Color maxRadiusColor = new Color(0.2f, 0.8f, 1f, 0.8f);
    [SerializeField] private Color minRadiusColor = new Color(1f, 0.3f, 0.2f, 0.9f);

    private EventInstance proximityInstance;
    private float currentProximityValue;
    private bool isInitialized;

    private void OnEnable()
    {
        if (playerTransform == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
        }

        StartProximityEvent();
    }

    private void OnDisable()
    {
        StopAndReleaseProximityEvent(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
    }

    private void OnDestroy()
    {
        StopAndReleaseProximityEvent(FMOD.Studio.STOP_MODE.IMMEDIATE);
    }

    private void Update()
    {
        if (!isInitialized || !proximityInstance.isValid())
            return;

        float targetProximity = 0f;

        if (playerTransform != null && targetMaskTransform != null && targetMaskTransform.gameObject.activeInHierarchy)
        {
            float distance = Vector2.Distance(playerTransform.position, targetMaskTransform.position);

            if (distance <= minDistance)
            {
                targetProximity = 1f;
            }
            else if (distance < maxDetectionRadius)
            {
                float normalized = 1f - ((distance - minDistance) / (maxDetectionRadius - minDistance));
                targetProximity = Mathf.Clamp01(proximityCurve.Evaluate(Mathf.Clamp01(normalized)));
            }
        }

        if (parameterSmoothSpeed > 0f)
        {
            currentProximityValue = Mathf.MoveTowards(
                currentProximityValue,
                targetProximity,
                parameterSmoothSpeed * Time.deltaTime
            );
        }
        else
        {
            currentProximityValue = targetProximity;
        }

        proximityInstance.setParameterByName(proximityParameterName, currentProximityValue);

        if (use3DPositioning && targetMaskTransform != null)
        {
            proximityInstance.set3DAttributes(RuntimeUtils.To3DAttributes(targetMaskTransform.position));
        }
    }

    /// <summary>
    /// Asigna o cambia la máscara objetivo actual del radar sonoro.
    /// Pasa null cuando se recoja la máscara o no haya objetivo activo.
    /// </summary>
    public void SetTargetMask(Transform newTarget, float? customRadius = null)
    {
        targetMaskTransform = newTarget;

        if (customRadius.HasValue && customRadius.Value > 0.1f)
        {
            maxDetectionRadius = customRadius.Value;
        }

        if (targetMaskTransform == null)
        {
            currentProximityValue = 0f;
            if (proximityInstance.isValid())
            {
                proximityInstance.setParameterByName(proximityParameterName, 0f);
            }
        }
    }

    /// <summary>
    /// Llama este método al recoger la máscara objetivo para silenciar/detener el radar.
    /// </summary>
    public void OnTargetMaskCollected()
    {
        SetTargetMask(null);
    }

    private void StartProximityEvent()
    {
        if (isInitialized && proximityInstance.isValid())
            return;

        if (proximityEvent.IsNull)
        {
            Debug.LogWarning("[TargetMaskProximityController] Proximity EventReference is not assigned.");
            return;
        }

        proximityInstance = RuntimeManager.CreateInstance(proximityEvent);
        currentProximityValue = 0f;
        proximityInstance.setParameterByName(proximityParameterName, currentProximityValue);

        if (use3DPositioning && targetMaskTransform != null)
        {
            proximityInstance.set3DAttributes(RuntimeUtils.To3DAttributes(targetMaskTransform.position));
        }

        proximityInstance.start();
        isInitialized = true;
    }

    private void StopAndReleaseProximityEvent(FMOD.Studio.STOP_MODE stopMode)
    {
        if (!isInitialized)
            return;

        if (proximityInstance.isValid())
        {
            proximityInstance.stop(stopMode);
            proximityInstance.release();
        }

        proximityInstance.clearHandle();
        isInitialized = false;
    }

    // ── Gizmos Debug ──────────────────────────────────────────────────────────

    private void OnDrawGizmos()
    {
        if (!showGizmos || drawOnlyWhenSelected) return;
        DrawGizmosInternal();
    }

    private void OnDrawGizmosSelected()
    {
        if (!showGizmos || !drawOnlyWhenSelected) return;
        DrawGizmosInternal();
    }

    private void DrawGizmosInternal()
    {
        // Centro del radio: la máscara objetivo (o el propio objeto si no hay target asignado)
        Vector3 center = targetMaskTransform != null ? targetMaskTransform.position : transform.position;

#if UNITY_EDITOR
        // Disco 2D (plano XY) con contorno nítido y relleno semitransparente
        Color fillMax = new Color(maxRadiusColor.r, maxRadiusColor.g, maxRadiusColor.b, 0.08f);
        UnityEditor.Handles.color = fillMax;
        UnityEditor.Handles.DrawSolidDisc(center, Vector3.forward, maxDetectionRadius);

        UnityEditor.Handles.color = maxRadiusColor;
        UnityEditor.Handles.DrawWireDisc(center, Vector3.forward, maxDetectionRadius);

        // Radio mínimo (zona de proximidad máxima = 1.0)
        Color fillMin = new Color(minRadiusColor.r, minRadiusColor.g, minRadiusColor.b, 0.25f);
        UnityEditor.Handles.color = fillMin;
        UnityEditor.Handles.DrawSolidDisc(center, Vector3.forward, minDistance);

        UnityEditor.Handles.color = minRadiusColor;
        UnityEditor.Handles.DrawWireDisc(center, Vector3.forward, minDistance);

        // Línea conectora hacia el jugador para ver la distancia y valor actual en tiempo real
        if (playerTransform != null)
        {
            Gizmos.color = Color.Lerp(Color.cyan, Color.red, currentProximityValue);
            Gizmos.DrawLine(center, playerTransform.position);

            string info = $"Proximity: {currentProximityValue:F2}\nDist: {Vector2.Distance(playerTransform.position, center):F1}m";
            GUIStyle labelStyle = new GUIStyle();
            labelStyle.normal.textColor = Color.white;
            labelStyle.fontSize = 11;
            labelStyle.fontStyle = FontStyle.Bold;
            UnityEditor.Handles.Label(center + Vector3.up * (minDistance + 0.3f), info, labelStyle);
        }
#else
        // Fallback estándar fuera del editor (esferas de alambre)
        Gizmos.color = maxRadiusColor;
        Gizmos.DrawWireSphere(center, maxDetectionRadius);

        Gizmos.color = minRadiusColor;
        Gizmos.DrawWireSphere(center, minDistance);
#endif
    }
}