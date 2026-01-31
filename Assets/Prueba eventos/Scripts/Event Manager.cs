using System;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static EventManager Instance;

    private void Awake()
    {
        //Singleton
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public event Action<int> EnActivarCapa;
    public void ActivarCapa(int capa)
    {
        EnActivarCapa?.Invoke(capa);
    }

    public event Action<int> EnDesactivarCapa;
    public void DesactivarCapa(int capa)
    {
        EnDesactivarCapa?.Invoke(capa);
    }
}
