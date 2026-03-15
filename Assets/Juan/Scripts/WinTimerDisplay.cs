using TMPro;
using UnityEngine;

public class WinTimerDisplay : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text currentRunTimeText;
    [SerializeField] private TMP_Text bestRunTimeText;

    [Header("Labels")]
    [SerializeField] private string currentLabel = "Tiempo de partida: ";
    [SerializeField] private string bestLabel = "Mejor tiempo: ";

    private void Start()
    {
        Refresh();
    }

    public void Refresh()
    {
        GameRunTimerManager manager = GameRunTimerManager.Instance;

        if (currentRunTimeText != null)
        {
            if (manager != null && manager.TryGetLatestCompletedRunSeconds(out float currentSeconds))
                currentRunTimeText.text = currentLabel + GameRunTimerManager.FormatRunTime(currentSeconds);
            else
                currentRunTimeText.text = string.Empty;
        }

        if (bestRunTimeText != null)
        {
            DifficultySelectManager.DifficultyMode lookupMode = DifficultySelectManager.DifficultyMode.Medium;
            bool hasLookupMode = false;

            if (manager != null)
                hasLookupMode = manager.TryGetLatestCompletedRunDifficulty(out lookupMode);

            if (!hasLookupMode && DifficultySelectManager.Instance != null)
            {
                lookupMode = DifficultySelectManager.Instance.CurrentMode;
                hasLookupMode = true;
            }

            if (manager != null && hasLookupMode && manager.TryGetBestRunSeconds(lookupMode, out float bestSeconds))
                bestRunTimeText.text = bestLabel + GameRunTimerManager.FormatRunTime(bestSeconds);
            else
                bestRunTimeText.text = string.Empty;
        }
    }
}
