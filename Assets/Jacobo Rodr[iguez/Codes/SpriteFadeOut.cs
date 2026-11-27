using System.Collections;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Aplica un fade out progresivo al canal alfa de un SpriteRenderer mediante una corrutina.
/// Puede iniciarse automáticamente en Start o llamando a StartFadeOut() desde código o UnityEvents.
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class SpriteFadeOut : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("Opcional: si se deja vacío, toma el SpriteRenderer de este mismo GameObject.")]
    [SerializeField] private SpriteRenderer targetSprite;

    [Header("Fade Settings")]
    [SerializeField, Min(0f)] private float delayBeforeFade = 0f;
    [SerializeField, Min(0.01f)] private float fadeDuration = 1f;
    [SerializeField] private bool fadeOnStart = false;
    [SerializeField] private bool useUnscaledTime = false;

    [Header("On Complete")]
    [SerializeField] private bool disableGameObjectOnComplete = false;
    [SerializeField] private bool destroyOnComplete = false;
    [SerializeField] private UnityEvent onFadeComplete;

    private Coroutine fadeRoutine;
    private float initialAlpha = 1f;

    private void Awake()
    {
        if (targetSprite == null)
            targetSprite = GetComponent<SpriteRenderer>();

        if (targetSprite != null)
            initialAlpha = targetSprite.color.a;
    }

    private void Start()
    {
        if (fadeOnStart)
            StartFadeOut();
    }

    /// <summary>
    /// Inicia el fade out hacia alfa 0. Reinicia la corrutina si ya estaba en curso.
    /// </summary>
    public void StartFadeOut()
    {
        if (targetSprite == null)
            targetSprite = GetComponent<SpriteRenderer>();

        if (targetSprite == null || !gameObject.activeInHierarchy)
            return;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeOutRoutine());
    }

    /// <summary>
    /// Detiene el fade actual y restaura el alfa original del sprite.
    /// </summary>
    public void ResetAlpha()
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }

        if (targetSprite != null)
            SetAlpha(initialAlpha);
    }

    private IEnumerator FadeOutRoutine()
    {
        if (delayBeforeFade > 0f)
        {
            if (useUnscaledTime)
                yield return new WaitForSecondsRealtime(delayBeforeFade);
            else
                yield return new WaitForSeconds(delayBeforeFade);
        }

        float startAlpha = targetSprite.color.a;
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            SetAlpha(Mathf.Lerp(startAlpha, 0f, t));
            yield return null;
        }

        SetAlpha(0f);
        fadeRoutine = null;

        onFadeComplete?.Invoke();

        if (destroyOnComplete)
            Destroy(gameObject);
        else if (disableGameObjectOnComplete)
            gameObject.SetActive(false);
    }

    private void SetAlpha(float alpha)
    {
        Color color = targetSprite.color;
        color.a = Mathf.Clamp01(alpha);
        targetSprite.color = color;
    }
}
