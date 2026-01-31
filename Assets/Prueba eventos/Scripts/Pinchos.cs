using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(Collider))]

public class Pinchos : ObjetoDeCapa
{
    private MeshRenderer miMeshRenderer;
    private Collider miCollider;

    

    void Start()
    {
        miMeshRenderer = GetComponent<MeshRenderer>();
        miCollider = GetComponent<Collider>();

        
    }

    public override void ActivarCapa(int capa)
    {
        for(int i = 0; i < misCapas.Length; i++)
        {
            if (misCapas[i] == capa)
            {
                miMeshRenderer.enabled = true;
                miCollider.enabled = true;

                misCapasActivas[i] = true;
                capasActivas++;
            }
        }
    }

    public override void DesctivarCapa(int capa)
    {
        

        for(int i = 0; i < misCapas.Length; i++)
        {
            if(misCapas[i] == capa)
            {
                misCapasActivas[i] = false;
                capasActivas--;
            }
        }

        if (base.capasActivas == 0)
        {
            miMeshRenderer.enabled = false;
            miCollider.enabled = false;
        }
    }
}
