using FMODUnity;
using UnityEngine;

/// <summary>
/// Controla los parámetros locales MaskID (1..4) y TargetProximity (0 = más cerca, 1 = más lejos)
/// sobre el StudioEventEmitter del mismo GameObject para las máscaras coleccionables.
/// </summary>
[RequireComponent(typeof(StudioEventEmitter))]
public class AudioCollectibleMask : MonoBehaviour
{
    [Header("Mask Identity")]
    [Tooltip("ID de la máscara coleccionable (1 a 4).")]
    [Range(1, 4)]
    [SerializeField] private int maskID = 1;

    [Header("FMOD Parameter Names")]
    [SerializeField] private string maskIdParameterName = "MaskID";
    [SerializeField] private string proximityParameterName = "TargetProximity";

    [Header("Proximity Detection (0 = Cerca, 1 = Lejos)")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private string playerTag = "Player";
    [SerializeField, Min(0.01f)] private float minDistance = 0.5f;
    [SerializeField, Min(0.1f)] private float maxDetectionRadius = 15f;
    [SerializeField] private AnimationCurve proximityCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f);
    [SerializeField, Min(0f)] private float parameterSmoothSpeed = 8f;

    [Header("Emitter Lifecycle")]
    [Tooltip("Si el StudioEventEmitter tiene Play Event = None, lo inicia automáticamente en Start tras configurar MaskID.")]
    [SerializeField] private bool autoPlayIfNoTrigger = true;
    [Tooltip("Detiene el emitter al destruir/recoger la máscara para evitar fugas de audio en loop.")]
    [SerializeField] private bool stopEmitterOnDestroy = true;

    [Header("Gizmos Debug")]
    [SerializeField] private bool showGizmos = true;
    [SerializeField] private bool drawOnlyWhenSelected = true;
    [SerializeField] private Color minRadiusColor = new Color(0.2f, 1f, 0.4f, 0.9f);
    [SerializeField] private Color maxRadiusColor = new Color(0.2f, 0.8f, 1f, 0.8f);

    private StudioEventEmitter emitter;
    private float currentProximity = 1f;
    private bool isQuitting;

    private void Awake()
    {
        emitter = GetComponent<StudioEventEmitter>();
        SyncMaskIdIntoEmitterParams();
    }

    private void Start()
    {
        EnsurePlayerReference();
        currentProximity = ComputeTargetProximity();

        if (emitter != null)
        {
            if (autoPlayIfNoTrigger && emitter.EventPlayTrigger == EmitterGameEvent.None && !emitter.IsPlaying())
                emitter.Play();

            ApplyParametersToEmitter(ignoreSeekSpeed: true);
        }
    }

    private void Update()
    {
        if (emitter == null)
            return;

        EnsurePlayerReference();

        float targetProximity = ComputeTargetProximity();
        if (parameterSmoothSpeed > 0f)
        {
            currentProximity = Mathf.MoveTowards(currentProximity, targetProximity, parameterSmoothSpeed * Time.deltaTime);
        }
        else
        {
            currentProximity = targetProximity;
        }

        ApplyParametersToEmitter(ignoreSeekSpeed: false);
    }

    private void OnApplicationQuit()
    {
        isQuitting = true;
    }

    private void OnDestroy()
    {
        if (!isQuitting && stopEmitterOnDestroy && emitter != null && emitter.IsPlaying())
        {
            emitter.Stop();
        }
    }

    public void SetMaskID(int newMaskID)
    {
        maskID = Mathf.Clamp(newMaskID, 1, 4);
        SyncMaskIdIntoEmitterParams();

        if (emitter != null)
            emitter.SetParameter(maskIdParameterName, maskID, true);
    }

    private void EnsurePlayerReference()
    {
        if (playerTransform != null || string.IsNullOrWhiteSpace(playerTag))
            return;

        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);
        if (playerObj != null)
            playerTransform = playerObj.transform;
    }

    private float ComputeTargetProximity()
    {
        if (playerTransform == null)
            return 1f;

        float distance = Vector2.Distance(transform.position, playerTransform.position);
        float safeMax = Mathf.Max(minDistance + 0.01f, maxDetectionRadius);

        // 0 = más cerca (<= minDistance), 1 = más lejos (>= maxDetectionRadius)
        float normalized = Mathf.InverseLerp(minDistance, safeMax, distance);
        return Mathf.Clamp01(proximityCurve.Evaluate(normalized));
    }

    private void ApplyParametersToEmitter(bool ignoreSeekSpeed)
    {
        if (emitter == null)
            return;

        if (!string.IsNullOrWhiteSpace(maskIdParameterName))
            emitter.SetParameter(maskIdParameterName, maskID, ignoreSeekSpeed);

        if (!string.IsNullOrWhiteSpace(proximityParameterName))
            emitter.SetParameter(proximityParameterName, currentProximity, ignoreSeekSpeed);
    }

    /// <summary>
    /// Asegura que MaskID esté presente en StudioEventEmitter.Params antes de Lookup/PlayInstance,
    /// evitando que al arrancar en ObjectStart tome un ID por defecto incorrecto.
    /// </summary>
    private void SyncMaskIdIntoEmitterParams()
    {
        if (emitter == null || string.IsNullOrWhiteSpace(maskIdParameterName))
            return;

        ParamRef[] existing = emitter.Params;
        if (existing != null)
        {
            for (int i = 0; i < existing.Length; i++)
            {
                if (existing[i] != null && string.Equals(existing[i].Name, maskIdParameterName, System.StringComparison.Ordinal))
                {
                    existing[i].Value = maskID;
                    return;
                }
            }
        }

        int oldLen = existing != null ? existing.Length : 0;
        ParamRef[] updated = new ParamRef[oldLen + 1];
        for (int i = 0; i < oldLen; i++)
            updated[i] = existing[i];

        updated[oldLen] = new ParamRef { Name = maskIdParameterName, Value = maskID };
        emitter.Params = updated;
    }

    private void OnValidate()
    {
        maskID = Mathf.Clamp(maskID, 1, 4);
        if (maxDetectionRadius <= minDistance)
            maxDetectionRadius = minDistance + 0.1f;

        if (emitter == null)
            emitter = GetComponent<StudioEventEmitter>();

        SyncMaskIdIntoEmitterParams();
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos || drawOnlyWhenSelected) return;
        DrawProximityGizmos();
    }

    private void OnDrawGizmosSelected()
    {
        if (!showGizmos || !drawOnlyWhenSelected) return;
        DrawProximityGizmos();
    }

    private void DrawProximityGizmos()
    {
        Vector3 center = transform.position;

#if UNITY_EDITOR
        UnityEditor.Handles.color = new Color(maxRadiusColor.r, maxRadiusColor.g, maxRadiusColor.b, 0.08f);
        UnityEditor.Handles.DrawSolidDisc(center, Vector3.forward, maxDetectionRadius);
        UnityEditor.Handles.color = maxRadiusColor;
        UnityEditor.Handles.DrawWireDisc(center, Vector3.forward, maxDetectionRadius);

        UnityEditor.Handles.color = new Color(minRadiusColor.r, minRadiusColor.g, minRadiusColor.b, 0.25f);
        UnityEditor.Handles.DrawSolidDisc(center, Vector3.forward, minDistance);
        UnityEditor.Handles.color = minRadiusColor;
        UnityEditor.Handles.DrawWireDisc(center, Vector3.forward, minDistance);
#else
        Gizmos.color = maxRadiusColor;
        Gizmos.DrawWireSphere(center, maxDetectionRadius);
        Gizmos.color = minRadiusColor;
        Gizmos.DrawWireSphere(center, minDistance);
#endif
    }
}
