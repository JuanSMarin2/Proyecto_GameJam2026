using UnityEngine;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

[DisallowMultipleComponent]
public class ButtonSound : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;

    [Header("Input")]
    [SerializeField] private bool includeKeyboard = true;
    [SerializeField] private bool includeMouseButtons = true;
    [SerializeField] private bool includeGamepad = true;

    private void Awake()
    {
        if (audioSource == null)
        {
            audioSource = FindFirstObjectByType<AudioSource>();
        }
    }

    private void Update()
    {
        if (audioSource == null || audioSource.clip == null)
        {
            return;
        }

        if (AnyButtonPressedThisFrame())
        {
            audioSource.PlayOneShot(audioSource.clip);
        }
    }

    private bool AnyButtonPressedThisFrame()
    {
#if ENABLE_INPUT_SYSTEM
        if (includeKeyboard && Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame)
        {
            return true;
        }

        if (includeMouseButtons && Mouse.current != null)
        {
            var mouse = Mouse.current;
            if (mouse.leftButton.wasPressedThisFrame || mouse.rightButton.wasPressedThisFrame || mouse.middleButton.wasPressedThisFrame)
            {
                return true;
            }

            if (mouse.forwardButton != null && mouse.forwardButton.wasPressedThisFrame)
            {
                return true;
            }

            if (mouse.backButton != null && mouse.backButton.wasPressedThisFrame)
            {
                return true;
            }
        }

        if (includeGamepad && Gamepad.current != null)
        {
            var gamepad = Gamepad.current;

            if (gamepad.buttonSouth.wasPressedThisFrame || gamepad.buttonNorth.wasPressedThisFrame || gamepad.buttonEast.wasPressedThisFrame || gamepad.buttonWest.wasPressedThisFrame)
            {
                return true;
            }

            if (gamepad.startButton.wasPressedThisFrame || gamepad.selectButton.wasPressedThisFrame)
            {
                return true;
            }

            if (gamepad.leftShoulder.wasPressedThisFrame || gamepad.rightShoulder.wasPressedThisFrame)
            {
                return true;
            }

            if (gamepad.leftTrigger.wasPressedThisFrame || gamepad.rightTrigger.wasPressedThisFrame)
            {
                return true;
            }

            if (gamepad.leftStickButton.wasPressedThisFrame || gamepad.rightStickButton.wasPressedThisFrame)
            {
                return true;
            }

            if (gamepad.dpad.up.wasPressedThisFrame || gamepad.dpad.down.wasPressedThisFrame || gamepad.dpad.left.wasPressedThisFrame || gamepad.dpad.right.wasPressedThisFrame)
            {
                return true;
            }
        }

        return false;
#else
        return Input.anyKeyDown;
#endif
    }
}
