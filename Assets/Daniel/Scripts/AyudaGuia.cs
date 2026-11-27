using UnityEngine;
using UnityEngine.Events;

public class AyudaGuia : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject panelUI;
    [SerializeField] private GameObject objetoGuiaADesactivar;
    [SerializeField] private bool Reactivable;
    [SerializeField] private bool ducksPause;

    [Header("Eventos")]
    public UnityEvent OnHelpActivated;
    public UnityEvent OnHelpDeactivated;

    private bool panelYaAbierto;
    private bool panelYaCerrado;

    private void Start()
    {
        if (panelUI != null)
            panelUI.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (panelUI == null) return;
        if (!Reactivable)
        {
            if (panelYaAbierto || panelYaCerrado) return;    
        }
        if (!collision.gameObject.CompareTag("Player")) return;
        objetoGuiaADesactivar.SetActive(false);
        panelUI.SetActive(true);
        panelYaAbierto = true;
            AudioManager.instance.SetPausedState(ducksPause);
        OnHelpActivated?.Invoke();

       /*  PlayerMovement playerMovement = collision.GetComponent<PlayerMovement>();
        if (playerMovement != null)
            playerMovement.BeginWaterFlow(); */
    }

    // Asignar esta funcion al OnClick del boton de cerrar.
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (panelUI == null) return;
        if (!collision.gameObject.CompareTag("Player")) return;

        CerrarPanelAyuda();
        if (Reactivable)
        {
            objetoGuiaADesactivar.SetActive(true);
        }
    }
    public void CerrarPanelAyuda()
    {
        if (panelUI != null)
            panelUI.SetActive(false);
        AudioManager.instance.SetPausedState(false);
        PlayerMovement playerMovement = FindPlayerMovement();
        if (playerMovement != null)
            playerMovement.EndWaterFlow();

        panelYaCerrado = true;
        OnHelpDeactivated?.Invoke();

        if (objetoGuiaADesactivar != null)
            objetoGuiaADesactivar.SetActive(false);
    }

    private PlayerMovement FindPlayerMovement()
    {
        if (panelUI == null)
            return null;

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        return player != null ? player.GetComponent<PlayerMovement>() : null;
    }
   
}
