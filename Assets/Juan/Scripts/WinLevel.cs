using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class WinLevel : MonoBehaviour
{
    [SerializeField] private string sceneName;
    [SerializeField] private float winDelay = 2f;
    [SerializeField] private string winTriggerName = "win";

   public GameObject[] gameObjectArray;

    private bool isWinning;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isWinning) return;
        if (!other.CompareTag("Player")) return;

        isWinning = true;
        StartCoroutine(WinAndLoad(other.gameObject));
    }

    private IEnumerator WinAndLoad(GameObject player)
    {
        PlayerMovement pm = player != null ? player.GetComponent<PlayerMovement>() : null;
foreach (GameObject go in gameObjectArray)
        {
            go.SetActive(false);
        }

        if (pm != null)
            pm.DisableMovementForWin();

        Animator a = player != null ? player.GetComponent<Animator>() : null;
        if (a != null)
            a.SetTrigger(winTriggerName);

        yield return new WaitForSeconds(winDelay);

        if (!string.IsNullOrWhiteSpace(sceneName))
            SceneManager.LoadScene(sceneName);
        else
            Debug.LogWarning("WinLevel: sceneName no asignado.");
    }
}
