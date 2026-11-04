using UnityEngine;

public class AyudaGuia : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject panelUI;
    [SerializeField] private GameObject objetoGuiaADesactivar;

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
        if (panelYaAbierto || panelYaCerrado) return;
        if (!collision.gameObject.CompareTag("Player")) return;
        
        panelUI.SetActive(true);
        panelYaAbierto = true;

        PlayerMovement playerMovement = collision.GetComponent<PlayerMovement>();
        if (playerMovement != null)
            playerMovement.BeginWaterFlow();
    }

    // Asignar esta funcion al OnClick del boton de cerrar.
    public void CerrarPanelAyuda()
    {
        if (panelUI != null)
            panelUI.SetActive(false);

        PlayerMovement playerMovement = FindPlayerMovement();
        if (playerMovement != null)
            playerMovement.EndWaterFlow();

        panelYaCerrado = true;

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
