using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    [SerializeField] private Bow bow;

    private const float ShotCooldown = 2f;
    private readonly HashSet<Collider2D> playerColliders = new HashSet<Collider2D>();
    private Coroutine shootingCoroutine;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Player"))
            return;

        if (bow == null)
        {
            Debug.LogWarning("PressurePlate: Bow no asignado.");
            return;
        }

        if (!playerColliders.Add(collision) || playerColliders.Count > 1)
            return;
        

        bow.Shoot();
        shootingCoroutine = StartCoroutine(HandleShooting());
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!playerColliders.Remove(collision) || playerColliders.Count > 0)
            return;

        if (shootingCoroutine != null)
        {
            StopCoroutine(shootingCoroutine);
            shootingCoroutine = null;
        }
    }

    private IEnumerator HandleShooting()
    {
        while (playerColliders.Count > 0)
        {
            yield return new WaitForSeconds(ShotCooldown);
            if (playerColliders.Count > 0)
                bow.Shoot();
        }

        shootingCoroutine = null;
    }

    private void OnDisable()
    {
        if (shootingCoroutine != null)
        {
            StopCoroutine(shootingCoroutine);
            shootingCoroutine = null;
        }

        playerColliders.Clear();
    }
}
