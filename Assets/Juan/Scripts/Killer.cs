using UnityEngine;
using System.Collections;

public class Killer : MonoBehaviour
{
     private float respawnDelay = 3f;
       private string deathTriggerName = "death";

    private bool isKilling;


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isKilling) return;
        if (!collision.gameObject.CompareTag("Player")) return;

        isKilling = true;
        StartCoroutine(KillAndRespawn(collision.gameObject));
    }

    private IEnumerator KillAndRespawn(GameObject player)
    {
        PlayerMovement pm = player != null ? player.GetComponent<PlayerMovement>() : null;
        if (pm != null)
            pm.DisableMovementForDeath();

        Animator playerAnimator = player != null ? player.GetComponent<Animator>() : null;
        if (playerAnimator != null)
            playerAnimator.SetTrigger(deathTriggerName);

        yield return new WaitForSeconds(respawnDelay);

        if (RespawnManager.Instance != null)
            RespawnManager.Instance.RespawnPlayer();

        // After respawn, try re-enabling movement (handles both same-object and recreated-player cases)
        GameObject currentPlayer = player != null ? player : GameObject.FindGameObjectWithTag("Player");
        PlayerMovement pmAfter = currentPlayer != null ? currentPlayer.GetComponent<PlayerMovement>() : null;
        if (pmAfter != null)
            pmAfter.EnableMovement();

        isKilling = false;
    }



}
