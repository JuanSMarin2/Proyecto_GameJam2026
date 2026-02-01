using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]

public class LayerController : ObjetoDeCapa
{
    private SpriteRenderer miMeshRenderer;
    private Collider2D miCollider;
    [SerializeField] private bool DesapearsOnLayer = false;
    [SerializeField, Range(0f, 1f)] private float inactiveOpacity = 0.3f;
    [Header("Materials")]
    [SerializeField] private Material activeMaterial;
    [SerializeField] private Material inactiveMaterial;

    

    void Start()
    {
        miMeshRenderer = GetComponent<SpriteRenderer>();
        miCollider = GetComponent<Collider2D>();

        // Estado inicial: si desaparece por capa, empieza activo; si no, empieza inactivo
        ApplyState(DesapearsOnLayer);
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

        miMeshRenderer.enabled = true;
        miCollider.enabled = active;

        SetOpacity(active ? 1f : inactiveOpacity);

        Material desired = active ? activeMaterial : inactiveMaterial;
        if (desired != null)
            miMeshRenderer.sharedMaterial = desired;
    }

    private void SetOpacity(float a)
    {
        if (miMeshRenderer == null) return;
        Color c = miMeshRenderer.color;
        c.a = Mathf.Clamp01(a);
        miMeshRenderer.color = c;
    }
}
