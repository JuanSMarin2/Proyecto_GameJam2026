using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameObject[] objetosPersistentes;

    private bool capa1Activa = false, capa2Activa = false, capa3Activa = false, capa4Activa = false;
    public int mascarasRecogidas = 0;



    

    private void Awake()
    {
        //singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public void Capa1(InputAction.CallbackContext cc)
    {
        if (!cc.performed) return;

        if(mascarasRecogidas < 1) return;

        if (capa1Activa)
        {
            Debug.Log("Desactiv� la capa 1");
            EventManager.Instance.DesactivarCapa(1);
            capa1Activa = false;
        }
        else
        {
            Debug.Log("Activ� la capa 1");
            EventManager.Instance.ActivarCapa(1);
            capa1Activa = true;
        }
    }

    public void Capa2(InputAction.CallbackContext cc)
    {
        if (!cc.performed) return;

        if (mascarasRecogidas < 2) return;

        if (capa2Activa)
        {
            EventManager.Instance.DesactivarCapa(2);
            capa2Activa = false;
        }
        else
        {
            EventManager.Instance.ActivarCapa(2);
            capa2Activa = true;
        }
    }

    public void Capa3(InputAction.CallbackContext cc)
    {
        if (!cc.performed) return;

        if (mascarasRecogidas < 3) return;

        if (capa3Activa)
        {
            EventManager.Instance.DesactivarCapa(3);
            capa3Activa = false;
        }
        else
        {
            EventManager.Instance.ActivarCapa(3);
            capa3Activa = true;
        }
    }

    public void Capa4(InputAction.CallbackContext cc)
    {
        if (!cc.performed) return;

        if (mascarasRecogidas < 4) return;

        if (capa4Activa)
        {
            EventManager.Instance.DesactivarCapa(4);
            capa4Activa = false;
        }
        else
        {
            EventManager.Instance.ActivarCapa(4);
            capa4Activa= true;
        }
    }

    private void Restart()
    {

    }

    public void MascaraRecogida(int mask)
    {
        mascarasRecogidas = mask ;
        switch (mascarasRecogidas)
        {
            case 1:
                SoundManager.PlaySound(SoundType.RecoleccionAfricana);
                break;
            case 2:
                SoundManager.PlaySound(SoundType.RecoleccionEuropea);
                break;
            case 3:
                SoundManager.PlaySound(SoundType.RecoleccionJaponesa);
                break;
            case 4:
                SoundManager.PlaySound(SoundType.RecoleccionLatina);
                break;
        }
    }
}
