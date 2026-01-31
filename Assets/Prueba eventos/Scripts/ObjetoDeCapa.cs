using UnityEngine;

public class ObjetoDeCapa : MonoBehaviour
{
    public int[] misCapas;
    protected bool[] misCapasActivas;
    protected int capasActivas;

    private void Awake()
    {
        EventManager.Instance.EnActivarCapa += ActivarCapa;
        EventManager.Instance.EnDesactivarCapa += DesctivarCapa;

        misCapasActivas = new bool[misCapas.Length];
        capasActivas = 0;
    }

    private void OnDisable()
    {
        EventManager.Instance.EnActivarCapa -= ActivarCapa;
        EventManager.Instance.EnDesactivarCapa -= DesctivarCapa;
    }

    public virtual void ActivarCapa(int capa)
    {
        for (int i = 0; i < misCapas.Length; i++)
        {
            if (misCapas[i] == capa)
            {
                misCapasActivas[i] = true;
                capasActivas++;
            }
        }
    }

    public virtual void DesctivarCapa(int capa)
    {
        for (int i = 0; i < misCapas.Length; i++)
        {
            if (misCapas[i] == capa)
            {
                misCapasActivas[i] = false;
                capasActivas--;
            }
        }

        if (capasActivas == 0)
        {
            
        }
    }
}
