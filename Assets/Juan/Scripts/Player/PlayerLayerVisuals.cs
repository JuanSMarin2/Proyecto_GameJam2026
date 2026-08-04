using System.Collections;
using UnityEngine;

public class PlayerLayerVisuals : MonoBehaviour
{
    private GameObject capa1;
    private GameObject capa2;
    private GameObject capa3;
    private GameObject capa4;

    private postprocessConfig postprocessConfig;
    private bool playMaskAnimationOnLayerChange = true;
    private float maskVisualDelay;

    private PlayerAnimation playerAnimation;

    private bool capa1Activa;
    private bool capa2Activa;
    private bool capa3Activa;
    private bool capa4Activa;

    private SpriteRenderer[] capa1Renderers;
    private SpriteRenderer[] capa2Renderers;
    private SpriteRenderer[] capa3Renderers;
    private SpriteRenderer[] capa4Renderers;

    private bool subscribedToLayers;
    private Coroutine layerVisualsRoutine;

    public void Configure(
        GameObject capa1,
        GameObject capa2,
        GameObject capa3,
        GameObject capa4,
        postprocessConfig postprocessConfig,
        bool playMaskAnimationOnLayerChange,
        float maskVisualDelay,
        PlayerAnimation playerAnimation)
    {
        this.capa1 = capa1;
        this.capa2 = capa2;
        this.capa3 = capa3;
        this.capa4 = capa4;
        this.postprocessConfig = postprocessConfig;
        this.playMaskAnimationOnLayerChange = playMaskAnimationOnLayerChange;
        this.maskVisualDelay = maskVisualDelay;
        this.playerAnimation = playerAnimation;

        CacheLayerRenderers();
        ApplyLayerVisuals();
    }

    private void Awake()
    {
        CacheLayerRenderers();
        ApplyLayerVisuals();
    }

    private void OnEnable()
    {
        TrySubscribeLayerEvents();
    }

    private void OnDisable()
    {
        UnsubscribeLayerEvents();
    }

    public void ApplyLayerVisuals()
    {
        SetRenderersEnabled(capa1Renderers, capa1Activa);
        SetRenderersEnabled(capa2Renderers, capa2Activa);
        SetRenderersEnabled(capa3Renderers, capa3Activa);
        SetRenderersEnabled(capa4Renderers, capa4Activa);
    }

    private void CacheLayerRenderers()
    {
        capa1Renderers = capa1 != null ? capa1.GetComponentsInChildren<SpriteRenderer>(true) : null;
        capa2Renderers = capa2 != null ? capa2.GetComponentsInChildren<SpriteRenderer>(true) : null;
        capa3Renderers = capa3 != null ? capa3.GetComponentsInChildren<SpriteRenderer>(true) : null;
        capa4Renderers = capa4 != null ? capa4.GetComponentsInChildren<SpriteRenderer>(true) : null;
    }

    private void SetRenderersEnabled(SpriteRenderer[] renderers, bool enabled)
    {
        if (renderers == null) return;

        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;
            renderers[i].enabled = enabled;
        }
    }

    private void TrySubscribeLayerEvents()
    {
        if (subscribedToLayers) return;
        if (EventManager.Instance == null) return;

        EventManager.Instance.EnActivarCapa += OnLayerActivated;
        EventManager.Instance.EnDesactivarCapa += OnLayerDeactivated;
        subscribedToLayers = true;
    }

    private void UnsubscribeLayerEvents()
    {
        if (!subscribedToLayers) return;
        if (EventManager.Instance == null) return;

        EventManager.Instance.EnActivarCapa -= OnLayerActivated;
        EventManager.Instance.EnDesactivarCapa -= OnLayerDeactivated;
        subscribedToLayers = false;
    }

    private void OnLayerActivated(int capa)
    {
        switch (capa)
        {
            case 1:
                capa1Activa = true;
                if (postprocessConfig != null)
                    postprocessConfig.ActivaMaskMagenta();
                break;
            case 2:
                capa2Activa = true;
                if (postprocessConfig != null)
                    postprocessConfig.ActivaMaskCyan();
                break;
            case 3:
                capa3Activa = true;
                if (postprocessConfig != null)
                    postprocessConfig.ActivaMaskYellow();
                break;
            case 4:
                capa4Activa = true;
                if (postprocessConfig != null)
                    postprocessConfig.ActivaMaskGreen();
                break;
        }

        QueueLayerVisualsUpdate();
    }

    private void OnLayerDeactivated(int capa)
    {
        switch (capa)
        {
            case 1: capa1Activa = false; break;
            case 2: capa2Activa = false; break;
            case 3: capa3Activa = false; break;
            case 4: capa4Activa = false; break;
        }

        if (!capa1Activa && !capa2Activa && !capa3Activa && !capa4Activa)
        {
            if (postprocessConfig != null)
                postprocessConfig.DesactivaMask();
        }

        QueueLayerVisualsUpdate();
    }

    private void QueueLayerVisualsUpdate()
    {
        if (layerVisualsRoutine != null)
        {
            StopCoroutine(layerVisualsRoutine);
            layerVisualsRoutine = null;
        }

        if (!playMaskAnimationOnLayerChange || playerAnimation == null || !playerAnimation.HasAnyAnimator)
        {
            ApplyLayerVisuals();
            return;
        }

        playerAnimation.TriggerMaskAnimation();
        layerVisualsRoutine = StartCoroutine(ApplyLayerVisualsAfterMask());
    }

    private IEnumerator ApplyLayerVisualsAfterMask()
    {
        if (maskVisualDelay <= 0f)
            yield return null;
        else
            yield return new WaitForSeconds(maskVisualDelay);

        ApplyLayerVisuals();
        layerVisualsRoutine = null;
    }
}