using System.Collections;
using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    [SerializeField] private Bow bow;
    [SerializeField] private bool isActive;

    private Coroutine shootingCoroutine;

    void Start()
    {
        isActive = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!isActive)
        {
            isActive = true;
            bow.Shoot(); // Disparo inmediato

            shootingCoroutine = StartCoroutine(HandleShooting());
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        isActive = false;

        if (shootingCoroutine != null)
        {
            StopCoroutine(shootingCoroutine);
            shootingCoroutine = null;
        }
    }

    private IEnumerator HandleShooting()
    {
        while (isActive)
        {
            yield return new WaitForSeconds(1f);
            bow.Shoot();
        }
    }
}
