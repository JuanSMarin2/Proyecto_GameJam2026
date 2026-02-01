using System.Collections;
using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    [SerializeField] private Bow bow;
    [Header("Burst")]
    [SerializeField] private int arrowsPerActivation = 5;
    [SerializeField] private float timeBetweenShots = 1f;
    [SerializeField] private bool shootImmediately = true;

    private static PressurePlate lastActivatedPlate;
    private Coroutine shootingCoroutine;
    private bool isShooting;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isShooting)
            return;

        // Esta misma placa no vuelve a disparar hasta que se active otra diferente
        if (lastActivatedPlate == this)
            return;

        if (bow == null)
        {
            Debug.LogWarning("PressurePlate: Bow no asignado.");
            return;
        }

        lastActivatedPlate = this;
        isShooting = true;

        SoundManager.PlaySound(SoundType.PlacaPresion);
        shootingCoroutine = StartCoroutine(HandleShooting());
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        // No se usa estado activo continuo: la placa dispara una ráfaga fija al activarse.
    }

    private IEnumerator HandleShooting()
    {
        int shots = Mathf.Max(0, arrowsPerActivation);
        if (shots == 0)
        {
            isShooting = false;
            shootingCoroutine = null;
            yield break;
        }

        if (shootImmediately)
        {
            bow.Shoot();
            shots--;
        }

        while (shots > 0)
        {
            yield return new WaitForSeconds(Mathf.Max(0f, timeBetweenShots));
            bow.Shoot();
            shots--;
        }

        isShooting = false;
        shootingCoroutine = null;
    }
}
