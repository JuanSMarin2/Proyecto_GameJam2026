using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Pause Menu")]
    [SerializeField] private GameObject pauseMenuRoot;

    [Header("Mask Images (1..4)")]
    [SerializeField] private Image[] maskImages = new Image[4];

    [Header("Alpha States")]
    [SerializeField, Range(0f, 1f)] private float alphaDesequipado = 0.35f;
    [SerializeField, Range(0f, 1f)] private float alphaEquipado = 1f;

    // Tracks layer state even if the image is currently inactive (mask not collected yet)
    private readonly bool[] layerActive = new bool[4];

    private int lastMaskCount = -1;

    private bool isPaused;

    private void OnEnable()
    {
        // Subscribe to layer events
        if (EventManager.Instance != null)
        {
            EventManager.Instance.EnActivarCapa += HandleActivarCapa;
            EventManager.Instance.EnDesactivarCapa += HandleDesactivarCapa;
        }

        // Initial refresh (best-effort)
        int masks = GameManager.Instance != null ? GameManager.Instance.mascarasRecogidas : 0;
        for (int i = 0; i < 4; i++)
        {
            int capa = i + 1;
            layerActive[i] = GameManager.Instance != null && GameManager.Instance.IsCapaActiva(capa);
        }
        lastMaskCount = -1;
        RefreshFromMaskCount(masks);

        if (pauseMenuRoot != null)
            pauseMenuRoot.SetActive(isPaused);
    }

    private void OnDisable()
    {
        if (EventManager.Instance != null)
        {
            EventManager.Instance.EnActivarCapa -= HandleActivarCapa;
            EventManager.Instance.EnDesactivarCapa -= HandleDesactivarCapa;
        }
    }

    private void Update()
    {
        if (WasEscapePressedThisFrame())
            TogglePause();

        if (GameManager.Instance == null) return;
        RefreshFromMaskCount(GameManager.Instance.mascarasRecogidas);
    }

    private static bool WasEscapePressedThisFrame()
    {
        if (Keyboard.current != null)
            return Keyboard.current.escapeKey.wasPressedThisFrame;

        return Input.GetKeyDown(KeyCode.Escape);
    }

    public void TogglePause()
    {
        SoundManager.PlaySound(SoundType.Leitmotif2);
        SetPaused(!isPaused);

    }

    public void ResumePause()
    {
        SetPaused(false);
    }

    public void GoToMainMenu()
    {
        SetPaused(false);
        SceneManager.LoadScene("MainMenu");
    }

    private void SetPaused(bool paused)
    {
        if (isPaused == paused)
            return;

        isPaused = paused;

        if (pauseMenuRoot != null)
            pauseMenuRoot.SetActive(isPaused);

        Time.timeScale = isPaused ? 0f : 1f;

        if (isPaused)
            SoundManager.LowerGlobalVolume();
        else
            SoundManager.RestoreGlobalVolume();
    }

    private void RefreshFromMaskCount(int maskCount)
    {
        int clamped = Mathf.Clamp(maskCount, 0, 4);
        if (clamped == lastMaskCount) return;

        lastMaskCount = clamped;
        RefreshMaskVisibility(clamped);
        RefreshAllAlphas();
    }

    private void HandleActivarCapa(int capa)
    {
        if (!TryIndexFromCapa(capa, out int index)) return;
        layerActive[index] = true;
        RefreshAlpha(index);
    }

    private void HandleDesactivarCapa(int capa)
    {
        if (!TryIndexFromCapa(capa, out int index)) return;
        layerActive[index] = false;
        RefreshAlpha(index);
    }

    private void RefreshMaskVisibility(int maskCount)
    {
        int clamped = Mathf.Clamp(maskCount, 0, 4);
        for (int i = 0; i < 4; i++)
        {
            Image img = GetImage(i);
            if (img == null) continue;

            bool shouldBeActive = clamped >= (i + 1);
            if (img.gameObject.activeSelf != shouldBeActive)
                img.gameObject.SetActive(shouldBeActive);
        }
    }

    private void RefreshAllAlphas()
    {
        for (int i = 0; i < 4; i++)
            RefreshAlpha(i);
    }

    private void RefreshAlpha(int index)
    {
        Image img = GetImage(index);
        if (img == null) return;
        if (!img.gameObject.activeInHierarchy) return; // only matters when visible

        float targetAlpha = layerActive[index] ? alphaEquipado : alphaDesequipado;
        Color c = img.color;
        c.a = Mathf.Clamp01(targetAlpha);
        img.color = c;
    }

    private Image GetImage(int index)
    {
        if (maskImages == null || maskImages.Length < 4) return null;
        if (index < 0 || index >= maskImages.Length) return null;
        return maskImages[index];
    }

    private static bool TryIndexFromCapa(int capa, out int index)
    {
        index = capa - 1;
        return index >= 0 && index < 4;
    }
}