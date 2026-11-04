using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class WindFlow : MonoBehaviour
{
    [Header("Wind")]
    [SerializeField] private Vector2 direction = Vector2.right;
    [SerializeField, Min(0f)] private float pushForce = 2f;
    [SerializeField, Min(0f)] private float activeInterval = 2f;
    [SerializeField, Min(0f)] private float inactiveInterval = 1f;
    [SerializeField] private bool startsActive = true;
    [SerializeField] private GameObject windVisual;
    [SerializeField, Min(0f)] private float fadeDuration = 0.25f;

    [Header("Audio")]
    [SerializeField] private SoundType windSound = SoundType.Viento;

    private readonly HashSet<PlayerMovement> playersInside = new HashSet<PlayerMovement>();
    private readonly HashSet<Box> boxesInside = new HashSet<Box>();
    private readonly HashSet<PlayerMovement> pushedPlayers = new HashSet<PlayerMovement>();
    private readonly HashSet<PlayerMovement> blockedPlayers = new HashSet<PlayerMovement>();
    private readonly Dictionary<PlayerMovement, float> blockedTimes = new Dictionary<PlayerMovement, float>();

    private bool isActive;
    private float intervalTimer;
    private Coroutine windVisualFadeRoutine;
    private SpriteRenderer[] windVisualRenderers;
    private float[] windVisualRendererAlphas;
    private CanvasGroup windVisualCanvasGroup;
    private float windVisualCanvasGroupAlpha;

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void Awake()
    {
        GetComponent<Collider2D>().isTrigger = true;
        isActive = startsActive;
        CacheWindVisual();
        UpdateWindVisual(true);
    }

    private void Update()
    {
        float interval = isActive ? activeInterval : inactiveInterval;
        if (interval <= 0f)
        {
            SetActive(!isActive);
            return;
        }

        intervalTimer += Time.deltaTime;
        if (intervalTimer < interval)
            return;

        intervalTimer = 0f;
        SetActive(!isActive);
    }

    private void FixedUpdate()
    {
        if (!isActive)
            return;

        Vector2 windDirection = direction.normalized;
        if (windDirection == Vector2.zero)
            return;

        UpdatePlayers(windDirection);
        UpdateBoxes(windDirection);
    }

    private void UpdatePlayers(Vector2 windDirection)
    {
        PlayerMovement[] activePlayers = new PlayerMovement[playersInside.Count];
        playersInside.CopyTo(activePlayers);

        for (int i = 0; i < activePlayers.Length; i++)
        {
            PlayerMovement player = activePlayers[i];
            if (player == null)
            {
                playersInside.Remove(player);
                pushedPlayers.Remove(player);
                blockedPlayers.Remove(player);
                blockedTimes.Remove(player);
                continue;
            }

            if (!pushedPlayers.Contains(player) && !blockedPlayers.Contains(player))
                BeginPushingPlayer(player);

            bool moved = player.MoveByWater(windDirection, pushForce, out bool blockedByObstacle);
            if (moved)
            {
                blockedTimes[player] = 0f;
                continue;
            }

            if (blockedByObstacle)
            {
                blockedPlayers.Add(player);
                StopPushingPlayer(player);
                continue;
            }

            float blockedTime = blockedTimes.TryGetValue(player, out float elapsed) ? elapsed : 0f;
            blockedTime += Time.fixedDeltaTime;
            blockedTimes[player] = blockedTime;
            if (blockedTime >= 0.5f)
            {
                blockedPlayers.Add(player);
                StopPushingPlayer(player);
            }
        }
    }

    private void UpdateBoxes(Vector2 windDirection)
    {
        Box[] activeBoxes = new Box[boxesInside.Count];
        boxesInside.CopyTo(activeBoxes);

        for (int i = 0; i < activeBoxes.Length; i++)
        {
            Box box = activeBoxes[i];
            if (box == null)
            {
                boxesInside.Remove(box);
                continue;
            }

            if (box.IsMoving)
                continue;

            box.MoveByWater(windDirection, pushForce);
        }
    }

    private void SetActive(bool active)
    {
        isActive = active;
        intervalTimer = 0f;
        UpdateWindVisual(false);

        if (!isActive)
        {
            PlayerMovement[] activePlayers = new PlayerMovement[pushedPlayers.Count];
            pushedPlayers.CopyTo(activePlayers);
            for (int i = 0; i < activePlayers.Length; i++)
                StopPushingPlayer(activePlayers[i]);
        }
        else
        {
            blockedPlayers.Clear();
        }
    }

    private void CacheWindVisual()
    {
        if (windVisual == null)
            return;

        windVisualRenderers = windVisual.GetComponentsInChildren<SpriteRenderer>(true);
        windVisualRendererAlphas = new float[windVisualRenderers.Length];
        for (int i = 0; i < windVisualRenderers.Length; i++)
            windVisualRendererAlphas[i] = windVisualRenderers[i].color.a;

        windVisualCanvasGroup = windVisual.GetComponent<CanvasGroup>();
        if (windVisualCanvasGroup != null)
            windVisualCanvasGroupAlpha = windVisualCanvasGroup.alpha;
    }

    private void UpdateWindVisual(bool immediate)
    {
        if (windVisual == null)
            return;

        if (windVisualFadeRoutine != null)
            StopCoroutine(windVisualFadeRoutine);

        float targetAlpha = isActive ? 1f : 0f;
        if (immediate || fadeDuration <= 0f)
        {
            SetWindVisualAlpha(targetAlpha);
            windVisualFadeRoutine = null;
            return;
        }

        windVisualFadeRoutine = StartCoroutine(FadeWindVisual(targetAlpha));
    }

    private IEnumerator FadeWindVisual(float targetAlpha)
    {
        float startingAlpha = GetWindVisualAlpha();
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeDuration);
            SetWindVisualAlpha(Mathf.Lerp(startingAlpha, targetAlpha, t));
            yield return null;
        }

        SetWindVisualAlpha(targetAlpha);
        windVisualFadeRoutine = null;
    }

    private float GetWindVisualAlpha()
    {
        if (windVisualCanvasGroup != null)
            return windVisualCanvasGroup.alpha / Mathf.Max(0.0001f, windVisualCanvasGroupAlpha);

        if (windVisualRenderers != null && windVisualRenderers.Length > 0)
            return windVisualRenderers[0].color.a / Mathf.Max(0.0001f, windVisualRendererAlphas[0]);

        return isActive ? 1f : 0f;
    }

    private void SetWindVisualAlpha(float alpha)
    {
        if (windVisualCanvasGroup != null)
            windVisualCanvasGroup.alpha = windVisualCanvasGroupAlpha * alpha;

        if (windVisualRenderers == null)
            return;

        for (int i = 0; i < windVisualRenderers.Length; i++)
        {
            if (windVisualRenderers[i] == null)
                continue;

            Color color = windVisualRenderers[i].color;
            color.a = windVisualRendererAlphas[i] * alpha;
            windVisualRenderers[i].color = color;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        RegisterObject(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        RegisterObject(other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        PlayerMovement player = other.GetComponentInParent<PlayerMovement>();
        if (player != null)
        {
            playersInside.Remove(player);
            blockedPlayers.Remove(player);
            StopPushingPlayer(player);
        }

        Box box = other.GetComponentInParent<Box>();
        if (box != null)
            boxesInside.Remove(box);
    }

    private void RegisterObject(Collider2D other)
    {
        PlayerMovement player = other.GetComponentInParent<PlayerMovement>();
        if (player != null)
        {
            playersInside.Add(player);
            if (isActive)
                BeginPushingPlayer(player);
        }

        Box box = other.GetComponentInParent<Box>();
        if (box != null)
            boxesInside.Add(box);
    }

    private void BeginPushingPlayer(PlayerMovement player)
    {
        if (!pushedPlayers.Add(player))
            return;

        blockedTimes[player] = 0f;
        player.BeginWaterFlow();
        SoundManager.PlaySound(windSound);
    }

    private void StopPushingPlayer(PlayerMovement player)
    {
        if (!pushedPlayers.Remove(player))
            return;

        blockedTimes.Remove(player);
        if (player != null)
            player.EndWaterFlow();
    }
}
