using UnityEngine;

public class RespawnManager : MonoBehaviour
{

    [SerializeField] private string sceneName;

    public static RespawnManager Instance { get; private set; }

    private void Awake()
    {
        // 2. Comprobar si ya existe una instancia
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Destruir duplicado
            return;
        }

        Instance = this;
      
    }
    




    public void RespawnPlayer()
    {
        Time.timeScale = 1f;

        // Justo antes de respawnear, asegurar que no quede ninguna capa activa.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.DesactivarTodasLasCapas();
        }
        else if (EventManager.Instance != null)
        {
            EventManager.Instance.DesactivarCapa(1);
            EventManager.Instance.DesactivarCapa(2);
            EventManager.Instance.DesactivarCapa(3);
            EventManager.Instance.DesactivarCapa(4);
        }

        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }


}
