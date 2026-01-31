using UnityEngine;

[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(Collider))]
public class Obstaculo : MonoBehaviour
{
    public int miCapa;

    private MeshRenderer miMeshRenderer;
    private Collider miBoxCollider;

    private void Awake()
    {
        EventManager.Instance.EnActivarCapa += ActivarCapa;
        EventManager.Instance.EnDesactivarCapa += DesctivarCapa;
    }

    private void OnDisable()
    {
        EventManager.Instance.EnActivarCapa -= ActivarCapa;
        EventManager.Instance.EnDesactivarCapa -= DesctivarCapa;
    }

    private void Start()
    {
        miMeshRenderer = GetComponent<MeshRenderer>();
        miBoxCollider = GetComponent<Collider>();
    }

    private void ActivarCapa(int capa)
    {
        if(miCapa == capa)
        {
            miBoxCollider.enabled = true;
            miMeshRenderer.enabled = true;
        }
    }

    private void DesctivarCapa(int capa)
    {
        if (miCapa == capa)
        {
            miBoxCollider.enabled = false;
            miMeshRenderer.enabled = false;
        }
    }
}
