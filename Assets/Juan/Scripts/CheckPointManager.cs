using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckPointManager : MonoBehaviour
{
    [SerializeField] private GameObject checkpointObject;
    [SerializeField] private string sceneName;
    [SerializeField] private GameObject playerObject;

    private void Start()
    {
        if (GameManager.Instance == null) return;
        if (!GameManager.Instance.hasCheckpoint) return;
        if (string.IsNullOrWhiteSpace(sceneName)) return;
        if (SceneManager.GetActiveScene().name != sceneName) return;
        if (checkpointObject == null || playerObject == null) return;

        playerObject.transform.position = checkpointObject.transform.position;
    }

    public void ActivateCheckpoint(GameObject player)
    {
        if (GameManager.Instance == null) return;
        if (playerObject != null && player != null && player != playerObject) return;
        if (string.IsNullOrWhiteSpace(sceneName)) return;
        if (SceneManager.GetActiveScene().name != sceneName) return;

        GameManager.Instance.hasCheckpoint = true;
    }
}
