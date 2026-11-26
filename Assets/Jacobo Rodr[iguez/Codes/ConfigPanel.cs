using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfigPanel : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider ambienceSlider;

    [Header("Value Text")]
    [SerializeField] private TMP_Text masterValueText;
    [SerializeField] private TMP_Text sfxValueText;
    [SerializeField] private TMP_Text musicValueText;
    [SerializeField] private TMP_Text ambienceValueText;

    [Header("Mute Icons")]
    [SerializeField] private Image MasterIconImage;
    [SerializeField] private Image sfxIconImage;
    [SerializeField] private Image musicIconImage;
    [SerializeField] private Image ambienceIconImage;



    [Header("Icon Sprites")]
    [SerializeField] private Sprite unmutedSprite;
    [SerializeField] private Sprite mutedSprite;

    private const float MutedThreshold = 0.0001f;

    private void OnEnable()
    {
        BindSliderEvents(true);
        PullValuesFromAudioSystem();
        RefreshUI();
    }

    private void OnDisable()
    {
        BindSliderEvents(false);
    }

    private void BindSliderEvents(bool bind)
    {
        if (masterSlider != null)
        {
            if (bind) masterSlider.onValueChanged.AddListener(OnMasterSliderChanged);
            else masterSlider.onValueChanged.RemoveListener(OnMasterSliderChanged);
        }
        if (sfxSlider != null)
        {
            if (bind) sfxSlider.onValueChanged.AddListener(OnSfxSliderChanged);
            else sfxSlider.onValueChanged.RemoveListener(OnSfxSliderChanged);
        }

        if (musicSlider != null)
        {
            if (bind) musicSlider.onValueChanged.AddListener(OnMusicSliderChanged);
            else musicSlider.onValueChanged.RemoveListener(OnMusicSliderChanged);
        }
        if (ambienceSlider != null)
        {
            if (bind) ambienceSlider.onValueChanged.AddListener(OnAmbienceSliderChanged);
            else ambienceSlider.onValueChanged.RemoveListener(OnAmbienceSliderChanged);
        }
    }

    private void PullValuesFromAudioSystem()
    {
        float sfx = AudioManager.instance != null ? AudioManager.instance.GetSfxVolume() : 1f;
        float music = AudioManager.instance != null ? AudioManager.instance.GetMusicVolume() : 1f;

        if (sfxSlider != null)
            sfxSlider.SetValueWithoutNotify(sfx);

        if (musicSlider != null)
            musicSlider.SetValueWithoutNotify(music);
    }

    private void OnMasterSliderChanged(float value)
    {
        if (AudioManager.instance != null)
            AudioManager.instance.SetMasterVolume(value);
            RefreshMasterUI(value);
    }

    private void OnSfxSliderChanged(float value)
    {
        if (AudioManager.instance != null)
            AudioManager.instance.SetSfxVolume(value);

        RefreshSfxUI(value);
    }

    private void OnMusicSliderChanged(float value)
    {
        if (AudioManager.instance != null)
            AudioManager.instance.SetMusicVolume(value);

        RefreshMusicUI(value);
    }
    private void OnAmbienceSliderChanged(float value)
    {
        if (AudioManager.instance != null)
            AudioManager.instance.SetAmbienceVolume(value);
            RefreshAmbienceUI(value);
    }

    public void ToggleSfxMute()
    {
        if (AudioManager.instance == null)
            return;

        float current = AudioManager.instance.GetSfxVolume();

        if (current <= MutedThreshold)
            AudioManager.instance.UnmuteSfxRestore();
        else
            AudioManager.instance.MuteSfx();

        float newValue = AudioManager.instance.GetSfxVolume();
        if (sfxSlider != null)
            sfxSlider.SetValueWithoutNotify(newValue);

        RefreshSfxUI(newValue);
    }
    public void ToggleAmbienceMute()
    {
        if (AudioManager.instance == null)
            return;

        float current = AudioManager.instance.GetAmbienceVolume();

        if (current <= MutedThreshold)
            AudioManager.instance.UnmuteAmbienceRestore();
        else
            AudioManager.instance.MuteAmbience();

        float newValue = AudioManager.instance.GetAmbienceVolume();
        if (ambienceSlider != null)
            ambienceSlider.SetValueWithoutNotify(newValue);
    }

    public void ToggleMusicMute()
    {
        if (AudioManager.instance == null)
            return;

        float current = AudioManager.instance.GetMusicVolume();

        if (current <= MutedThreshold)
            AudioManager.instance.UnmuteMusicRestore();
        else
            AudioManager.instance.MuteMusic();

        float newValue = AudioManager.instance.GetMusicVolume();
        if (musicSlider != null)
            musicSlider.SetValueWithoutNotify(newValue);

        RefreshMusicUI(newValue);
    }

    private void RefreshUI()
    {
        float master = AudioManager.instance != null ? AudioManager.instance.GetMasterVolume() : 1f;
        float sfx = AudioManager.instance != null ? AudioManager.instance.GetSfxVolume() : 1f;
        float music = AudioManager.instance != null ? AudioManager.instance.GetMusicVolume() : 1f;
        float ambience = AudioManager.instance != null ? AudioManager.instance.GetAmbienceVolume() : 1f;

        RefreshMasterUI(master);
        RefreshSfxUI(sfx);
        RefreshMusicUI(music);
        RefreshAmbienceUI(ambience);
        
    }


    private void RefreshMasterUI(float value)
    {
        if (masterValueText != null)
            masterValueText.text = ToPercentText(value);

        if (MasterIconImage != null)
            MasterIconImage.sprite = value <= MutedThreshold ? mutedSprite : unmutedSprite;
    }
    private void RefreshSfxUI(float value)
    {
        if (sfxValueText != null)
            sfxValueText.text = ToPercentText(value);

        if (sfxIconImage != null)
            sfxIconImage.sprite = value <= MutedThreshold ? mutedSprite : unmutedSprite;
    }

    private void RefreshAmbienceUI(float value)
    {
        if (ambienceValueText != null)
            ambienceValueText.text = ToPercentText(value);

        if (ambienceIconImage != null)
            ambienceIconImage.sprite = value <= MutedThreshold ? mutedSprite : unmutedSprite;
    }

    private void RefreshMusicUI(float value)
    {
        if (musicValueText != null)
            musicValueText.text = ToPercentText(value);

        if (musicIconImage != null)
            musicIconImage.sprite = value <= MutedThreshold ? mutedSprite : unmutedSprite;
    }

    private static string ToPercentText(float value)
    {
        int percent = Mathf.RoundToInt(Mathf.Clamp01(value) * 100f);
        return percent + "%";
    }
}
