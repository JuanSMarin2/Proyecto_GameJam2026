using UnityEngine;
using UnityEngine.Rendering.Universal;

public class luzdinamica : MonoBehaviour
{
    [Header("Referencia de luz")]
    [SerializeField] private Light2D spotLight2D;
    [SerializeField] private bool buscarEnHijos = true;

    [Header("Parpadeo tipo antorcha")]
    [SerializeField] private float intensidadBase = 1.2f;
    [SerializeField] private float amplitud = 0.35f;
    [SerializeField] private float velocidad = 2.5f;
    [SerializeField] private float ruido = 0.15f;

    private float semillaRuido;

    private void Awake()
    {
        if (spotLight2D == null)
        {
            spotLight2D = buscarEnHijos ? GetComponentInChildren<Light2D>() : GetComponent<Light2D>();
        }

        if (spotLight2D == null)
        {
            Debug.LogWarning("No se encontro un componente Light2D en este objeto.", this);
            enabled = false;
            return;
        }

        semillaRuido = Random.Range(0f, 9999f);
        spotLight2D.intensity = intensidadBase;
    }

    private void Update()
    {
        float onda = Mathf.Sin(Time.time * velocidad) * amplitud;

        // Perlin noise evita un parpadeo mecanico y hace el efecto mas natural.
        float ruidoSuave = (Mathf.PerlinNoise(semillaRuido, Time.time * velocidad * 0.6f) - 0.5f) * 2f * ruido;

        float nuevaIntensidad = intensidadBase + onda + ruidoSuave;
        spotLight2D.intensity = Mathf.Max(0f, nuevaIntensidad);
    }
}
