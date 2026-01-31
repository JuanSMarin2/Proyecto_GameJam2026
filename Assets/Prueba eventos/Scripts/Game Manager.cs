using System.Threading;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private bool capa1Activa = true, capa2Activa = true, capa3Activa = true, capa4Activa = true;

    public void Capa1(InputAction.CallbackContext cc)
    {
        if (cc.performed)
        {
            if (capa1Activa)
            {
                EventManager.Instance.DesactivarCapa(1);
                capa1Activa = false;
            }
            else
            {
                EventManager.Instance.ActivarCapa(1);
                capa1Activa = true;
            }
        }
    }

    public void Capa2(InputAction.CallbackContext cc)
    {
        if (cc.performed)
        {
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
    }

    public void Capa3(InputAction.CallbackContext cc)
    {
        if (cc.performed)
        {
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
    }

    public void Capa4(InputAction.CallbackContext cc)
    {
        if (cc.performed)
        {
            if (capa4Activa)
            {
                EventManager.Instance.DesactivarCapa(4);
                capa4Activa = false;
            }
            else
            {
                EventManager.Instance.ActivarCapa(4);
                capa4Activa = true;
            }
        }
    }
}
