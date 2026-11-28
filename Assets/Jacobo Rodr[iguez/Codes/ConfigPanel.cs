using System;
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
        BindSlider(masterSlider, OnMasterSliderChanged, bind);
        BindSlider(sfxSlider, OnSfxSliderChanged, bind);
        BindSlider(musicSlider, OnMusicSliderChanged, bind);
        BindSlider(ambienceSlider, OnAmbienceSliderChanged, bind);
    }

    private static void BindSlider(Slider slider, UnityEngine.Events.UnityAction<float> callback, bool bind)
    {
        if (slider == null)
            return;

        if (bind)
            slider.onValueChanged.AddListener(callback);
        else
            slider.onValueChanged.RemoveListener(callback);
    }

    private void PullValuesFromAudioSystem()
    {
        float master = AudioManager.instance != null ? AudioManager.instance.GetMasterVolume() : 1f;
        float sfx = AudioManager.instance != null ? AudioManager.instance.GetSfxVolume() : 1f;
        float music = AudioManager.instance != null ? AudioManager.instance.GetMusicVolume() : 1f;
        float ambience = AudioManager.instance != null ? AudioManager.instance.GetAmbienceVolume() : 1f;

        if (masterSlider != null)
            masterSlider.SetValueWithoutNotify(master);

        if (sfxSlider != null)
            sfxSlider.SetValueWithoutNotify(sfx);

        if (musicSlider != null)
            musicSlider.SetValueWithoutNotify(music);

        if (ambienceSlider != null)
            ambienceSlider.SetValueWithoutNotify(ambience);
    }

    private void OnMasterSliderChanged(float value)
    {
        if (AudioManager.instance != null)
            AudioManager.instance.SetMasterVolume(value);

        RefreshChannelUI(masterValueText, MasterIconImage, value);
    }

    private void OnSfxSliderChanged(float value)
    {
        if (AudioManager.instance != null)
            AudioManager.instance.SetSfxVolume(value);

        RefreshChannelUI(sfxValueText, sfxIconImage, value);
    }

    private void OnMusicSliderChanged(float value)
    {
        if (AudioManager.instance != null)
            AudioManager.instance.SetMusicVolume(value);

        RefreshChannelUI(musicValueText, musicIconImage, value);
    }

    private void OnAmbienceSliderChanged(float value)
    {
        if (AudioManager.instance != null)
            AudioManager.instance.SetAmbienceVolume(value);

        RefreshChannelUI(ambienceValueText, ambienceIconImage, value);
    }

    public void ToggleMasterMute()
    {
        if (AudioManager.instance == null)
            return;

        ToggleChannelMute(
            AudioManager.instance.GetMasterVolume,
            AudioManager.instance.MuteMaster,
            AudioManager.instance.UnmuteMasterRestore,
            masterSlider,
            masterValueText,
            MasterIconImage);
    }

    public void ToggleSfxMute()
    {
        if (AudioManager.instance == null)
            return;

        ToggleChannelMute(
            AudioManager.instance.GetSfxVolume,
            AudioManager.instance.MuteSfx,
            AudioManager.instance.UnmuteSfxRestore,
            sfxSlider,
            sfxValueText,
            sfxIconImage);
    }

    public void ToggleAmbienceMute()
    {
        if (AudioManager.instance == null)
            return;

        ToggleChannelMute(
            AudioManager.instance.GetAmbienceVolume,
            AudioManager.instance.MuteAmbience,
            AudioManager.instance.UnmuteAmbienceRestore,
            ambienceSlider,
            ambienceValueText,
            ambienceIconImage);
    }

    public void ToggleMusicMute()
    {
        if (AudioManager.instance == null)
            return;

        ToggleChannelMute(
            AudioManager.instance.GetMusicVolume,
            AudioManager.instance.MuteMusic,
            AudioManager.instance.UnmuteMusicRestore,
            musicSlider,
            musicValueText,
            musicIconImage);
    }

    private void ToggleChannelMute(
        Func<float> getVolume,
        Action muteAction,
        Action unmuteAction,
        Slider slider,
        TMP_Text valueText,
        Image iconImage)
    {
        float current = getVolume();

        if (current <= MutedThreshold)
            unmuteAction();
        else
            muteAction();

        float newValue = getVolume();
        if (slider != null)
            slider.SetValueWithoutNotify(newValue);

        RefreshChannelUI(valueText, iconImage, newValue);
    }

    private void RefreshUI()
    {
        float master = AudioManager.instance != null ? AudioManager.instance.GetMasterVolume() : 1f;
        float sfx = AudioManager.instance != null ? AudioManager.instance.GetSfxVolume() : 1f;
        float music = AudioManager.instance != null ? AudioManager.instance.GetMusicVolume() : 1f;
        float ambience = AudioManager.instance != null ? AudioManager.instance.GetAmbienceVolume() : 1f;

        RefreshChannelUI(masterValueText, MasterIconImage, master);
        RefreshChannelUI(sfxValueText, sfxIconImage, sfx);
        RefreshChannelUI(musicValueText, musicIconImage, music);
        RefreshChannelUI(ambienceValueText, ambienceIconImage, ambience);
    }

    private void RefreshChannelUI(TMP_Text valueText, Image iconImage, float value)
    {
        if (valueText != null)
            valueText.text = ToPercentText(value);

        if (iconImage != null)
            iconImage.sprite = value <= MutedThreshold ? mutedSprite : unmutedSprite;
    }

    private static string ToPercentText(float value)
    {
        int percent = Mathf.RoundToInt(Mathf.Clamp01(value) * 100f);
        return percent + "%";
    }
}
