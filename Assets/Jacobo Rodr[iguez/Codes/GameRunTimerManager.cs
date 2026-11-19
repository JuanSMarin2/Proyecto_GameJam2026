using UnityEngine;
using UnityEngine.SceneManagement;

public class GameRunTimerManager : MonoBehaviour
{
    [SerializeField] private string winSceneName = "Ganaste";

    public static GameRunTimerManager Instance { get; private set; }

    private const string PrefBestRunSecondsPrefix = "BestRunSeconds_";

    private bool runTimerActive;
    private float runStartRealtime;
    private DifficultySelectManager.DifficultyMode runningDifficulty;
    private bool hasLatestCompletedRun;
    private float latestCompletedRunSeconds;
    private DifficultySelectManager.DifficultyMode latestCompletedRunDifficulty;

    public static GameRunTimerManager EnsureInstance()
    {
        if (Instance != null)
            return Instance;

        GameObject go = new GameObject(nameof(GameRunTimerManager));
        return go.AddComponent<GameRunTimerManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == winSceneName)
            CompleteRunTimerIfActive();
    }

    public void StartRunForDifficulty(DifficultySelectManager.DifficultyMode mode)
    {
        runTimerActive = true;
        runningDifficulty = mode;
        runStartRealtime = Time.realtimeSinceStartup;
        ClearLatestCompletedRun();
    }

    public bool TryGetLatestCompletedRunSeconds(out float seconds)
    {
        seconds = latestCompletedRunSeconds;
        return hasLatestCompletedRun;
    }

    public bool TryGetLatestCompletedRunDifficulty(out DifficultySelectManager.DifficultyMode mode)
    {
        mode = latestCompletedRunDifficulty;
        return hasLatestCompletedRun;
    }

    public bool TryGetBestRunSeconds(DifficultySelectManager.DifficultyMode mode, out float seconds)
    {
        string key = GetBestTimeKey(mode);

        if (PlayerPrefs.HasKey(key))
        {
            seconds = Mathf.Max(0f, PlayerPrefs.GetFloat(key));
            return true;
        }

        seconds = 0f;
        return false;
    }

    public static string FormatRunTime(float seconds)
    {
        seconds = Mathf.Max(0f, seconds);
        int total = Mathf.FloorToInt(seconds);
        int minutes = total / 60;
        int sec = total % 60;
        int centiseconds = Mathf.FloorToInt((seconds - total) * 100f);
        return $"{minutes:00}:{sec:00}.{centiseconds:00}";
    }

    private void CompleteRunTimerIfActive()
    {
        if (!runTimerActive)
            return;

        float elapsed = Mathf.Max(0f, Time.realtimeSinceStartup - runStartRealtime);
        runTimerActive = false;

        latestCompletedRunSeconds = elapsed;
        hasLatestCompletedRun = true;
        latestCompletedRunDifficulty = runningDifficulty;

        string key = GetBestTimeKey(runningDifficulty);

        if (!PlayerPrefs.HasKey(key))
        {
            PlayerPrefs.SetFloat(key, elapsed);
            PlayerPrefs.Save();
            return;
        }

        float best = Mathf.Max(0f, PlayerPrefs.GetFloat(key));
        if (elapsed < best)
        {
            PlayerPrefs.SetFloat(key, elapsed);
            PlayerPrefs.Save();
        }
    }

    private void ClearLatestCompletedRun()
    {
        hasLatestCompletedRun = false;
        latestCompletedRunSeconds = 0f;
        latestCompletedRunDifficulty = DifficultySelectManager.DifficultyMode.Medium;
    }

    private static string GetBestTimeKey(DifficultySelectManager.DifficultyMode mode)
    {
        return PrefBestRunSecondsPrefix + mode;
    }
}
