using UnityEngine;
using UnityEngine.SceneManagement;


public class MenuManager : MonoBehaviour
{

    
    [SerializeField]
    GameObject panelCreditos;
  



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (GameManager.Instance.juegoTerminado)
        {
            panelCreditos.SetActive(true);

        }
         

        GameManager.Instance.mascarasRecogidas = 0;
        GameManager.Instance.juegoTerminado = false;



    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Jugar(){
        Time.timeScale = 1f;
        SceneManager.LoadScene("Intro");
    }

    public void JugarFacil()
    {
        Time.timeScale = 1f;

        if (DifficultySelectManager.Instance != null)
        {
            DifficultySelectManager.Instance.SelectEasyAndStartGame();
            return;
        }

        Jugar();
    }

    public void JugarMedio()
    {
        Time.timeScale = 1f;

        if (DifficultySelectManager.Instance != null)
        {
            DifficultySelectManager.Instance.SelectMediumAndStartGame();
            return;
        }

        Jugar();
    }

    public void JugarDificil()
    {
        Time.timeScale = 1f;

        if (DifficultySelectManager.Instance != null)
        {
            DifficultySelectManager.Instance.SelectHardAndStartGame();
            return;
        }

        Jugar();
    }

    public void AbreCreditos(){
        panelCreditos.SetActive(true);
    }

    
    public void CierraCreditos(){
        panelCreditos.SetActive(false);
    }

    public void CierraApp(){
        Application.Quit();
    }


}
