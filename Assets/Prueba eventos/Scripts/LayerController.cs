using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Collider2D))]

public class LayerController : ObjetoDeCapa
{
    private SpriteRenderer miMeshRenderer;
    private Collider2D miCollider;
    [SerializeField] private bool DesapearsOnLayer = false;
    [SerializeField, Range(0f, 1f)] private float inactiveOpacity = 0.3f;

    

    void Start()
    {
        miMeshRenderer = GetComponent<SpriteRenderer>();
        miCollider = GetComponent<Collider2D>();

        if (DesapearsOnLayer)
        {
            miMeshRenderer.enabled = true;
            miCollider.enabled = true;
            SetOpacity(1f);
        }
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
                    miMeshRenderer.enabled = true;
                    miCollider.enabled = true;
                    SetOpacity(1f);
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
            miMeshRenderer.enabled = true;
            miCollider.enabled = visible;
            SetOpacity(visible ? 1f : inactiveOpacity);
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

            if (base.capasActivas == 0)
            {
                miMeshRenderer.enabled = true;
                miCollider.enabled = false;
                SetOpacity(inactiveOpacity);
            }
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
            miMeshRenderer.enabled = true;
            miCollider.enabled = visible;
            SetOpacity(visible ? 1f : inactiveOpacity);
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
