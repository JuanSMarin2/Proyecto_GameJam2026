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
    }

    // Asignar esta funcion al OnClick del boton de cerrar.
    public void CerrarPanelAyuda()
    {
        if (panelUI != null)
            panelUI.SetActive(false);

        panelYaCerrado = true;

        if (objetoGuiaADesactivar != null)
            objetoGuiaADesactivar.SetActive(false);
    }
   
}
