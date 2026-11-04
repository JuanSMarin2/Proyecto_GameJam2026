using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class ButtonSound : MonoBehaviour, IPointerDownHandler, ISubmitHandler
{
    [Header("Audio")]
    [SerializeField] private SoundType buttonSound = SoundType.UIButtonSound;
    [SerializeField, Range(0f, 1f)] private float volume = 1f;

    private Selectable selectable;
    private Button button;
    private int lastSoundFrame = -1;

    private void Awake()
    {
        selectable = GetComponent<Selectable>();
        button = GetComponent<Button>();
        if (selectable == null)
        {
            Debug.LogWarning($"[{nameof(ButtonSound)}] Debe estar en el mismo GameObject que un control UI Selectable (por ejemplo, Button o Slider).", this);
            enabled = false;
        }
    }

    private void OnEnable()
    {
        if (button != null)
            button.onClick.AddListener(PlayButtonSound);
    }

    private void OnDisable()
    {
        if (button != null)
            button.onClick.RemoveListener(PlayButtonSound);
    }

    // Puede usarse también desde un evento OnClick/OnValueChanged del Inspector.
    public void PlayButtonSound()
    {
        if (selectable == null || !selectable.IsInteractable())
            return;

        // Evita duplicar el audio si además está enlazado a OnClick/OnValueChanged.
        if (lastSoundFrame == Time.frameCount)
            return;

        lastSoundFrame = Time.frameCount;
        SoundManager.PlaySound(buttonSound, null, volume);
    }

    // Los Button usan onClick; los demás Selectable (como Slider) su pulsación inicial.
    public void OnPointerDown(PointerEventData eventData)
    {
        if (button == null && eventData.button == PointerEventData.InputButton.Left)
            PlayButtonSound();
    }

    // Cubre la activación mediante teclado o mando cuando el control está seleccionado.
    public void OnSubmit(BaseEventData eventData)
    {
        PlayButtonSound();
    }
}
