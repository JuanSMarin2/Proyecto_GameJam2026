using UnityEngine;

public class CheckPoint : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private CheckPointManager checkPointManager;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Checkpoint trigger entered by: " + other.gameObject.name);
        checkPointManager.ActivateCheckpoint(other.gameObject);
    }
}
