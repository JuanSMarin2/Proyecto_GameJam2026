using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private CheckPointManager checkPointManager;
    [SerializeField] private bool snapCameraOnRespawn = true;

    private bool triggered;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        
        if (other == null || !other.CompareTag(playerTag)) return;
        if (checkPointManager == null) return;
        Debug.Log("Checkpoint alcanzado por: " + (other != null ? other.name : "null"));
        checkPointManager.ActivateCheckpoint(other.gameObject, snapCameraOnRespawn);
        triggered = true;
    }
}
