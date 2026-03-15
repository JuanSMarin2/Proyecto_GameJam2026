using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DifficultySelectManager : MonoBehaviour
{
    public enum DifficultyMode
    {
        Easy,
        Medium,
        Hard
    }

    [Serializable]
    public class BossDifficultySettings
    {
        public int bossHealth = 7;
        public float bossLaunchForce = 6f;
    }

    [Header("Scene Flow")]
    [SerializeField] private string level1SceneName = "Lvl1";
    [SerializeField] private string level2SceneName = "Lvl2";
    [SerializeField] private string level3SceneName = "Lvl3";
    [SerializeField] private string preBossSceneName = "PreBoss";

    [Header("Boss Difficulty (serialized)")]
    [SerializeField] private BossDifficultySettings easy = new BossDifficultySettings { bossHealth = 5, bossLaunchForce = 5f };
    [SerializeField] private BossDifficultySettings medium = new BossDifficultySettings { bossHealth = 7, bossLaunchForce = 6f };
    [SerializeField] private BossDifficultySettings hard = new BossDifficultySettings { bossHealth = 9, bossLaunchForce = 8f };

    [Header("Default")]
    [SerializeField] private DifficultyMode defaultMode = DifficultyMode.Medium;

    public static DifficultySelectManager Instance { get; private set; }
    public DifficultyMode CurrentMode { get; private set; }

    public static event Action<DifficultyMode> OnDifficultyChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        CurrentMode = defaultMode;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "MainMenu")
            SetDifficulty(DifficultyMode.Medium);
    }

    public void SetEasyMode() => SetDifficulty(DifficultyMode.Easy);
    public void SetMediumMode() => SetDifficulty(DifficultyMode.Medium);
    public void SetHardMode() => SetDifficulty(DifficultyMode.Hard);

    public void SetDifficulty(DifficultyMode mode)
    {
        CurrentMode = mode;
        OnDifficultyChanged?.Invoke(CurrentMode);
    }

    public BossDifficultySettings GetCurrentBossSettings()
    {
        switch (CurrentMode)
        {
            case DifficultyMode.Easy: return easy;
            case DifficultyMode.Hard: return hard;
            default: return medium;
        }
    }

    public string ResolveNextSceneFromLevelFlow(string currentSceneName, string fallbackSceneName)
    {
        if (string.IsNullOrWhiteSpace(currentSceneName))
            return fallbackSceneName;

        if (currentSceneName == level1SceneName)
        {
            if (CurrentMode == DifficultyMode.Easy)
                return preBossSceneName;

            return level2SceneName;
        }

        if (currentSceneName == level2SceneName)
        {
            if (CurrentMode == DifficultyMode.Hard)
                return level3SceneName;

            return preBossSceneName;
        }

        if (currentSceneName == level3SceneName)
            return preBossSceneName;

        return fallbackSceneName;
    }
}
