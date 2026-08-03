using UnityEngine;

public class PanelMusica : MonoBehaviour
{
    [SerializeField] private float esperaInicial = 1.5f;
    [SerializeField] private float velocidadDesvanecimiento = 0.5f;

    // Persiste entre recargas de escena durante la misma ejecucion del juego.
    private static bool panelYaMostradoEnSesion;

    private CanvasGroup canvasGroup;
    private float temporizadorEspera;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        canvasGroup.alpha = Mathf.Clamp01(canvasGroup.alpha);
    }

    private void OnEnable()
    {
        if (panelYaMostradoEnSesion)
        {
            Destroy(gameObject);
            return;
        }

        panelYaMostradoEnSesion = true;

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        // Mantiene el panel totalmente visible durante el tiempo indicado.
        canvasGroup.alpha = 1f;
        temporizadorEspera = Mathf.Max(0f, esperaInicial);
    }

    private void Update()
    {
        if (canvasGroup.alpha <= 0f)
        {
            Destroy(gameObject);
            return;
        }

        if (temporizadorEspera > 0f)
        {
            temporizadorEspera -= Time.deltaTime;
            return;
        }

        canvasGroup.alpha -= velocidadDesvanecimiento * Time.deltaTime;
        if (canvasGroup.alpha <= 0f)
        {
            canvasGroup.alpha = 0f;
            Destroy(gameObject);
        }
    }
}
