using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class MenuMusicPlayer : MonoBehaviour
{
    [Header("Config")]
    [SerializeField] private SoundsSO soundsSO;
    [SerializeField] private SoundType menuMusicSound = SoundType.MusicaBase;
    [SerializeField] private bool useAmbientSound;
    [SerializeField] private SoundType ambientSound = SoundType.CapaBase;
    [SerializeField] private bool playOnStart = true;

    private AudioSource musicSource;
    private AudioSource ambientSource;
    private float musicConfiguredVolume = 1f;
    private float ambientConfiguredVolume = 1f;

    private void Awake()
    {
        musicSource = GetComponent<AudioSource>();
        musicSource.playOnAwake = false;
        musicSource.loop = true;

        ambientSource = gameObject.AddComponent<AudioSource>();
        ambientSource.playOnAwake = false;
        ambientSource.loop = true;

        SetupFromSoundsSO();
    }

    private void Start()
    {
        if (!playOnStart)
            return;

        if (musicSource != null && musicSource.clip != null && !musicSource.isPlaying)
            musicSource.Play();

        if (useAmbientSound && ambientSource != null && ambientSource.clip != null && !ambientSource.isPlaying)
            ambientSource.Play();
    }

    private void Update()
    {
        float musicMultiplier = SoundManager.GetEffectiveMusicVolume();

        if (musicSource != null)
            musicSource.volume = musicConfiguredVolume * musicMultiplier;

        if (ambientSource != null)
            ambientSource.volume = (useAmbientSound ? ambientConfiguredVolume : 0f) * musicMultiplier;
    }

    public void PlayMusic()
    {
        if (musicSource != null && musicSource.clip != null && !musicSource.isPlaying)
            musicSource.Play();

        if (useAmbientSound && ambientSource != null && ambientSource.clip != null && !ambientSource.isPlaying)
            ambientSource.Play();
    }

    public void StopMusic()
    {
        if (musicSource != null)
            musicSource.Stop();

        if (ambientSource != null)
            ambientSource.Stop();
    }

    private void SetupFromSoundsSO()
    {
        if (musicSource == null)
            return;

        if (soundsSO == null || soundsSO.sounds == null)
        {
            Debug.LogWarning("[MenuMusicPlayer] Missing SoundsSO reference.");
            return;
        }

        SetupSource(musicSource, menuMusicSound, ref musicConfiguredVolume, "menu music");

        if (useAmbientSound)
            SetupSource(ambientSource, ambientSound, ref ambientConfiguredVolume, "ambient");
        else if (ambientSource != null)
            ambientSource.clip = null;
    }

    private void SetupSource(AudioSource target, SoundType soundType, ref float configuredVolume, string label)
    {
        if (target == null)
            return;

        int index = (int)soundType;
        if (index < 0 || index >= soundsSO.sounds.Length)
        {
            Debug.LogWarning($"[MenuMusicPlayer] Missing SoundsSO entry for {soundType} ({label}).");
            target.clip = null;
            return;
        }

        SoundList list = soundsSO.sounds[index];
        configuredVolume = Mathf.Clamp01(list.volume);
        target.outputAudioMixerGroup = list.mixer;

        if (list.sounds == null || list.sounds.Length == 0)
        {
            Debug.LogWarning($"[MenuMusicPlayer] No clips assigned for {soundType} ({label}).");
            target.clip = null;
            return;
        }

        target.clip = list.sounds[0];
    }
}
