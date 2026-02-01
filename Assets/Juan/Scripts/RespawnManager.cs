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

        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }


}
