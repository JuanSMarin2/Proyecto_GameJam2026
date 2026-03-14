using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [Header("Pause Menu")]
    [SerializeField] private GameObject pauseMenuRoot;

    [Header("Audio")]
    [SerializeField, Range(0f, 1f)] private float pauseDuckingVolume = 0.25f;

    [Header("Mask Images (1..4)")]
    [SerializeField] private Image[] maskImages = new Image[4];

    [Header("Mask Sprites")]
    [SerializeField] private Sprite[] lockedSprites = new Sprite[4];
    [SerializeField] private Sprite[] unlockedSprites = new Sprite[4];
    [SerializeField] private Sprite[] equippedSprites = new Sprite[4];

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


    private void Start()
    {
        SoundManager.RestoreGlobalVolume();
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
            SoundManager.LowerGlobalVolume(pauseDuckingVolume);
        else
            SoundManager.RestoreGlobalVolume();
    }

    private void RefreshFromMaskCount(int maskCount)
    {
        int clamped = Mathf.Clamp(maskCount, 0, 4);
        if (clamped == lastMaskCount) return;

        lastMaskCount = clamped;
        RefreshAllMaskSprites(clamped);
    }

    private void HandleActivarCapa(int capa)
    {
        if (!TryIndexFromCapa(capa, out int index)) return;
        layerActive[index] = true;
        RefreshMaskSprite(index);
    }

    private void HandleDesactivarCapa(int capa)
    {
        if (!TryIndexFromCapa(capa, out int index)) return;
        layerActive[index] = false;
        RefreshMaskSprite(index);
    }

    private void RefreshAllMaskSprites(int maskCount)
    {
        int clamped = Mathf.Clamp(maskCount, 0, 4);
        for (int i = 0; i < 4; i++)
        {
            RefreshMaskSprite(i, clamped);
        }
    }

    private void RefreshMaskSprite(int index)
    {
        int maskCount = GameManager.Instance != null ? GameManager.Instance.mascarasRecogidas : 0;
        RefreshMaskSprite(index, Mathf.Clamp(maskCount, 0, 4));
    }

    private void RefreshMaskSprite(int index, int maskCount)
    {
        Image img = GetImage(index);
        if (img == null) return;

        bool isCollected = maskCount >= (index + 1);
        Sprite target = null;

        if (!isCollected)
            target = GetSprite(lockedSprites, index);
        else if (layerActive[index])
            target = GetSprite(equippedSprites, index);
        else
            target = GetSprite(unlockedSprites, index);

        if (target != null)
            img.sprite = target;

        if (!img.gameObject.activeSelf)
            img.gameObject.SetActive(true);
    }

    private Image GetImage(int index)
    {
        if (maskImages == null || maskImages.Length < 4) return null;
        if (index < 0 || index >= maskImages.Length) return null;
        return maskImages[index];
    }

    private static Sprite GetSprite(Sprite[] sprites, int index)
    {
        if (sprites == null || sprites.Length < 4) return null;
        if (index < 0 || index >= sprites.Length) return null;
        return sprites[index];
    }

    private static bool TryIndexFromCapa(int capa, out int index)
    {
        index = capa - 1;
        return index >= 0 && index < 4;
    }
}