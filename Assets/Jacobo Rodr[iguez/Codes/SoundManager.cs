using System;
using UnityEngine;

/// <summary>
/// Puente de compatibilidad hacia AudioManager (FMOD).
/// Toda la lógica de audio y volumen ha sido centralizada y sintetizada en AudioManager.cs.
/// </summary>
public class SoundManager : MonoBehaviour
{
    [SerializeField] private SoundsSO SO;
    public static SoundManager instance { get; private set; }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    #region Volume Management (Encaminado a AudioManager)

    public static float GetSfxVolume()
    {
        return AudioManager.instance != null ? AudioManager.instance.GetSfxVolume() : AudioManager.SfxVolume;
    }

    public static float GetMusicVolume()
    {
        return AudioManager.instance != null ? AudioManager.instance.GetMusicVolume() : AudioManager.MusicVolume;
    }

    public static float GetEffectiveSfxVolume()
    {
        return AudioManager.instance != null ? AudioManager.instance.GetEffectiveSfxVolume() : 1f;
    }

    public static float GetEffectiveMusicVolume()
    {
        return AudioManager.instance != null ? AudioManager.instance.GetEffectiveMusicVolume() : 1f;
    }

    public static void SetSfxVolume(float value01)
    {
        if (AudioManager.instance != null)
            AudioManager.instance.SetSfxVolume(value01);
        else
            AudioManager.SfxVolume = value01;
    }

    public static void SetMusicVolume(float value01)
    {
        if (AudioManager.instance != null)
            AudioManager.instance.SetMusicVolume(value01);
        else
            AudioManager.MusicVolume = value01;
    }

    public static void MuteSfx()
    {
        if (AudioManager.instance != null)
            AudioManager.instance.MuteSfx();
    }

    public static void UnmuteSfxRestore()
    {
        if (AudioManager.instance != null)
            AudioManager.instance.UnmuteSfxRestore();
    }

    public static void MuteMusic()
    {
        if (AudioManager.instance != null)
            AudioManager.instance.MuteMusic();
    }

    public static void UnmuteMusicRestore()
    {
        if (AudioManager.instance != null)
            AudioManager.instance.UnmuteMusicRestore();
    }

    public static void SetGlobalVolume(float value01)
    {
        if (AudioManager.instance != null)
            AudioManager.instance.SetGlobalVolume(value01);
    }

    public static void LowerGlobalVolume(float value01 = 0.25f)
    {
        if (AudioManager.instance != null)
            AudioManager.instance.LowerGlobalVolume(value01);
    }

    public static void RestoreGlobalVolume()
    {
        if (AudioManager.instance != null)
            AudioManager.instance.RestoreGlobalVolume();
    }

    #endregion

    #region PlaySound (Encaminado a FMOD / AudioManager)

    public static void PlaySound(SoundType sound, AudioSource source = null, float volume = 1f)
    {
        if (AudioManager.instance != null)
        {
            Vector3 pos = source != null ? source.transform.position : Vector3.zero;
            AudioManager.instance.PlaySound(sound, pos);
            return;
        }

        // Fallback legado si AudioManager aún no está instanciado pero existe el SoundsSO nativo
        if (instance != null && instance.SO != null && instance.SO.sounds != null)
        {
            int index = (int)sound;
            if (index >= 0 && index < instance.SO.sounds.Length)
            {
                SoundList soundList = instance.SO.sounds[index];
                if (soundList.sounds != null && soundList.sounds.Length > 0)
                {
                    AudioClip randomClip = soundList.sounds[UnityEngine.Random.Range(0, soundList.sounds.Length)];
                    if (source != null)
                    {
                        source.outputAudioMixerGroup = soundList.mixer;
                        source.volume = volume * soundList.volume;
                        source.PlayOneShot(randomClip);
                    }
                }
            }
        }
    }

    #endregion
}

[Serializable]
public struct SoundList
{
    [HideInInspector] public string name;
    [Range(0, 1)] public float volume;
    public UnityEngine.Audio.AudioMixerGroup mixer;
    public AudioClip[] sounds;
}
