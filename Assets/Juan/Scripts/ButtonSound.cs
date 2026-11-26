using UnityEngine;
using UnityEngine.UI;
using FMODUnity;

[DisallowMultipleComponent]
[RequireComponent(typeof(Button))]
public class ButtonSound : MonoBehaviour
{
    [Header("Audio (FMOD)")]
    [SerializeField] private EventReference soundEvent;

    private Button cachedButton;

    private void Awake()
    {
        cachedButton = GetComponent<Button>();
    }

    private void OnEnable()
    {
        if (cachedButton == null)
            cachedButton = GetComponent<Button>();

        if (cachedButton != null)
            cachedButton.onClick.AddListener(PlayButtonSound);
    }

    private void OnDisable()
    {
        if (cachedButton != null)
            cachedButton.onClick.RemoveListener(PlayButtonSound);
    }

    private void PlayButtonSound()
    {
        if (AudioManager.instance == null)
            return;

        EventReference evt = !soundEvent.IsNull
            ? soundEvent
            : (FMODEvents.instance != null ? FMODEvents.instance.UIButtonSoundEvent : default);

        if (!evt.IsNull)
        {
            AudioManager.instance.PlayOneShot(evt, transform.position);
        }
        else if (FMODEvents.instance == null)
        {
            Debug.LogWarning("No sound event assigned for button press and FMODEvents instance is null.");
        }
    }
}
