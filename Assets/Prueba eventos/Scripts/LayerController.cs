using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]

public class LayerController : ObjetoDeCapa
{
    private SpriteRenderer miMeshRenderer;
    private Collider2D miCollider;
    private Rigidbody2D miRigidbody;
    [SerializeField] private bool DesapearsOnLayer = false;
    [SerializeField, Range(0f, 1f)] private float inactiveOpacity = 0.3f;
    [Header("Materials")]
    [SerializeField] private Material activeMaterial;
    [SerializeField] private Material inactiveMaterial;
    [Header("Movement")]
    [SerializeField] private MonoBehaviour[] movementScripts;
    [SerializeField] private bool lockTransformWhenInactive = true;
    [SerializeField] private bool lockRotationWhenInactive = false;
    [SerializeField] private bool disableAllScriptsWhenInactive = false;

    private bool rbSimulatedDefault;
    private RigidbodyType2D rbBodyTypeDefault;
    private RigidbodyConstraints2D rbConstraintsDefault;

    private MonoBehaviour[] cachedBehaviours;
    private bool isActiveState;
    private Vector3 lockedPosition;
    private Quaternion lockedRotation;

    

    void Start()
    {
        miMeshRenderer = GetComponent<SpriteRenderer>();
        miCollider = GetComponent<Collider2D>();
        miRigidbody = GetComponent<Rigidbody2D>();

        cachedBehaviours = GetComponents<MonoBehaviour>();

        if (miRigidbody != null)
        {
            rbSimulatedDefault = miRigidbody.simulated;
            rbBodyTypeDefault = miRigidbody.bodyType;
            rbConstraintsDefault = miRigidbody.constraints;
        }

        // Estado inicial: si desaparece por capa, empieza activo; si no, empieza inactivo
        ApplyState(DesapearsOnLayer);
    }

    private void LateUpdate()
    {
        if (!lockTransformWhenInactive) return;
        if (isActiveState) return;

        // Lock transform even if some script tries to move it in Update
        transform.position = lockedPosition;
        if (lockRotationWhenInactive)
            transform.rotation = lockedRotation;
    }

    public override void ActivarCapa(int capa)
    {
        if (!DesapearsOnLayer)
        {
            for (int i = 0; i < misCapas.Length; i++)
            {
                if (misCapas[i] == capa)
                {
                    if (!misCapasActivas[i])
                    {
                        misCapasActivas[i] = true;
                        capasActivas++;
                    }
                    ApplyState(true);
                }
            }
        }
        else
        {
            for (int i = 0; i < misCapas.Length; i++)
            {
                if (misCapas[i] == capa)
                {
                    if (!misCapasActivas[i])
                    {
                        misCapasActivas[i] = true;
                        capasActivas++;
                    }
                }
            }

            bool visible = capasActivas < misCapas.Length;
            ApplyState(visible);
        }
    }

    public override void DesctivarCapa(int capa)
    {
        
        if (!DesapearsOnLayer)
        {
            for (int i = 0; i < misCapas.Length; i++)
            {
                if (misCapas[i] == capa)
                {
                    if (misCapasActivas[i])
                    {
                        misCapasActivas[i] = false;
                        capasActivas--;
                    }
                }
            }

            ApplyState(base.capasActivas > 0);
        }
        else
        {
            for (int i = 0; i < misCapas.Length; i++)
            {
                if (misCapas[i] == capa)
                {
                    if (misCapasActivas[i])
                    {
                        misCapasActivas[i] = false;
                        capasActivas--;
                    }
                }
            }
            bool visible = capasActivas < misCapas.Length;
            ApplyState(visible);
        }
    }

    private void ApplyState(bool active)
    {
        if (miMeshRenderer == null || miCollider == null) return;

        isActiveState = active;
        if (!active)
        {
            lockedPosition = transform.position;
            lockedRotation = transform.rotation;
        }

        miMeshRenderer.enabled = true;
        miCollider.enabled = active;

        SetOpacity(active ? 1f : inactiveOpacity);

        Material desired = active ? activeMaterial : inactiveMaterial;
        if (desired != null)
            miMeshRenderer.sharedMaterial = desired;

        SetMovementEnabled(active);
    }

    private void SetMovementEnabled(bool enabled)
    {
        // Disable movement scripts (configured in Inspector)
        if (movementScripts != null)
        {
            for (int i = 0; i < movementScripts.Length; i++)
            {
                if (movementScripts[i] == null) continue;
                movementScripts[i].enabled = enabled;
            }
        }

        // Optional: disable every script on this GameObject (except this controller)
        if (disableAllScriptsWhenInactive && cachedBehaviours != null)
        {
            for (int i = 0; i < cachedBehaviours.Length; i++)
            {
                MonoBehaviour b = cachedBehaviours[i];
                if (b == null) continue;
                if (b == this) continue;
                // Keep base ObjetoDeCapa logic alive (event subscription)
                if (b is ObjetoDeCapa) continue;
                b.enabled = enabled;
            }
        }

        // Freeze physics-based movement
        if (miRigidbody != null)
        {
            if (enabled)
            {
                miRigidbody.simulated = rbSimulatedDefault;
                miRigidbody.bodyType = rbBodyTypeDefault;
                miRigidbody.constraints = rbConstraintsDefault;
            }
            else
            {
                miRigidbody.linearVelocity = Vector2.zero;
                miRigidbody.angularVelocity = 0f;
                miRigidbody.simulated = false;
            }
        }
    }

    private void SetOpacity(float a)
    {
        if (miMeshRenderer == null) return;
        Color c = miMeshRenderer.color;
        c.a = Mathf.Clamp01(a);
        miMeshRenderer.color = c;
    }
}
