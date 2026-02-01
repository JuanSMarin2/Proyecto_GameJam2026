using Unity.VisualScripting;
using UnityEngine;

public class Máscara : MonoBehaviour
{
    public bool recogida = false;

    private Collider2D miCollider;


    [SerializeField] private int idMascara;

    private void Start()
    {


        if (GameManager.Instance.mascarasRecogidas >= idMascara)
        {
            Destroy(this.gameObject);
        }

        miCollider = GetComponent<Collider2D>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Trigger de mask");
        if(collision.gameObject.tag == "Player"){
            Debug.Log("Mascara recogida");
            GameManager.Instance.MascaraRecogida(idMascara);
            Destroy(this.gameObject);

        }
    }

    
}
