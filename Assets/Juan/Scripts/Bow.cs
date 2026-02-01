using UnityEngine;

public class Bow : MonoBehaviour
{
    [Header("Direction Flags")]
    [SerializeField] private bool horizontal = true;
    [SerializeField] private bool positive = true;

    [Header("Shooting")]
    [SerializeField] private GameObject arrowPrefab;
    [SerializeField] private float arrowSpeed = 8f;
    [SerializeField] private Transform shootPoint;

    // Call this to shoot an arrow
    public void Shoot()
    {
        if (arrowPrefab == null)
        {
            Debug.LogWarning("Bow: arrowPrefab no asignado.");
            return;
        }

        Vector2 dir;
        if (horizontal)
            dir = positive ? Vector2.right : Vector2.left;
        else
            dir = positive ? Vector2.up : Vector2.down;

        Vector3 spawnPos = shootPoint != null ? shootPoint.position : transform.position;
        // right=90, up=180, left=270, down=0
        float zRot = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg + 90f;
        Quaternion rot = Quaternion.Euler(0f, 0f, zRot);
           SoundManager.PlaySound(SoundType.Dardos);
        GameObject arrowGO = Instantiate(arrowPrefab, spawnPos, rot);

        Arrow arrow = arrowGO.GetComponent<Arrow>();
        if (arrow != null)
        {

          
         
          
            arrow.Launch(dir, arrowSpeed);
        }
        else
        {
            // Fallback if prefab doesn't have Arrow component
            Rigidbody2D rb = arrowGO.GetComponent<Rigidbody2D>();
            if (rb != null)
                rb.linearVelocity = dir.normalized * arrowSpeed;
            arrowGO.transform.rotation = rot;
            Destroy(arrowGO, 10f);
        }
    }
}
