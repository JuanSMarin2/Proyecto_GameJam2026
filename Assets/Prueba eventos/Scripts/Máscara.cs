using UnityEngine;

public class Máscara : MonoBehaviour
{
    public bool recogida = false;

    private Collider2D miCollider;

    private void Start()
    {
        miCollider = GetComponent<Collider2D>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.tag == "Jugador"){
            GameManager.Instance.MascaraRecogida();
        }
    }
}
