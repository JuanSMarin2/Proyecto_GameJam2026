using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CheckPointManager : MonoBehaviour
{
    public static event Action<Transform> OnPlayerRespawnedAtCheckpoint;

    private static bool hasSavedCameraSnapPosition;
    private static Vector3 savedCameraSnapPosition;
    private static bool snapCameraOnRespawn;

    [SerializeField] private GameObject checkpointObject;
    [SerializeField] private string sceneName;
    [SerializeField] private GameObject playerObject;
    [SerializeField] private Camera cameraToSnap;

    private bool checkpointTriggered;

    

    private void Start()
    {
        if (GameManager.Instance == null) return;
        if (!GameManager.Instance.hasCheckpoint) return;
        if (string.IsNullOrWhiteSpace(sceneName)) return;
        if (SceneManager.GetActiveScene().name != sceneName) return;
        if (checkpointObject == null || playerObject == null) return;

        playerObject.transform.position = checkpointObject.transform.position;
        Debug.Log("Player respawned at checkpoint: " + checkpointObject.name + " in scene: " + sceneName);
        StartCoroutine(NotifyRespawnNextFrame(playerObject.transform));
    }

    public void ActivateCheckpoint(GameObject player, bool enableCameraSnap)
    {
        if (GameManager.Instance == null) return;
        if (checkpointTriggered) return;
        if (playerObject != null && player != null && player != playerObject) return;
        if (string.IsNullOrWhiteSpace(sceneName)) return;
        if (SceneManager.GetActiveScene().name != sceneName) return;

        if (enableCameraSnap)
        {
            if (cameraToSnap == null)
                cameraToSnap = Camera.main;

            if (cameraToSnap != null)
            {
                savedCameraSnapPosition = cameraToSnap.transform.position;
                hasSavedCameraSnapPosition = true;
                snapCameraOnRespawn = true;
            }
            else
            {
                hasSavedCameraSnapPosition = false;
                snapCameraOnRespawn = false;
            }
        }
        else
        {
            hasSavedCameraSnapPosition = false;
            snapCameraOnRespawn = false;
        }

        GameManager.Instance.hasCheckpoint = true;
        checkpointTriggered = true;
    }

    public static bool TryGetCheckpointCameraSnapPosition(out Vector3 snapPosition)
    {
        snapPosition = Vector3.zero;

        if (GameManager.Instance == null) return false;
        if (!GameManager.Instance.hasCheckpoint) return false;
        if (!snapCameraOnRespawn) return false;
        if (!hasSavedCameraSnapPosition) return false;

        snapPosition = savedCameraSnapPosition;
        return true;
    }

    public static void ClearSavedCheckpointState()
    {
        hasSavedCameraSnapPosition = false;
        savedCameraSnapPosition = Vector3.zero;
        snapCameraOnRespawn = false;
    }

    private IEnumerator NotifyRespawnNextFrame(Transform playerTransform)
    {
        // Espera 1 frame para que Start/OnEnable de cámara y otros sistemas ya estén suscritos.
        yield return null;

        OnPlayerRespawnedAtCheckpoint?.Invoke(playerTransform);
    }
}
