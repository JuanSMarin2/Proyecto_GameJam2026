using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Añade ButtonSound a todos los Button de las escenas cargadas.
/// El enlace al evento onClick se realiza dentro de ButtonSound.
/// </summary>
public static class UIButtonSoundAutoBinder
{
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void InstallForLoadedScenes()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        for (int i = 0; i < SceneManager.sceneCount; i++)
            AddSoundsToButtons(SceneManager.GetSceneAt(i));
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        AddSoundsToButtons(scene);
    }

    private static void AddSoundsToButtons(Scene scene)
    {
        if (!scene.isLoaded)
            return;

        GameObject[] roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            Button[] buttons = roots[i].GetComponentsInChildren<Button>(true);
            for (int j = 0; j < buttons.Length; j++)
            {
                if (buttons[j].GetComponent<ButtonSound>() == null)
                    buttons[j].gameObject.AddComponent<ButtonSound>();
            }
        }
    }
}
