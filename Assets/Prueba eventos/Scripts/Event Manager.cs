using System;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        Debug.Log(Instance.gameObject);
    }

    public event Action<int> EnActivarCapa;
    public void ActivarCapa(int capa)
    {
        Debug.Log("quiero activar capa " + capa);
        EnActivarCapa?.Invoke(capa);
    }

    public event Action<int> EnDesactivarCapa;
    public void DesactivarCapa(int capa)
    {
        Debug.Log("quiero desactivar capa " + capa);
        EnDesactivarCapa?.Invoke(capa);
    }
}
