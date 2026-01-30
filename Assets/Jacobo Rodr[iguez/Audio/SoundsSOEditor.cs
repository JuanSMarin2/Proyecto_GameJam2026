//Author: Small Hedge Games
//Updated: 13/06/2024

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Audio;


    [CustomEditor(typeof(SoundsSO))]
    public class SoundsSOEditor : Editor
    {
        private void OnEnable()
        {
            foreach (var currentTarget in targets)
            {
                var soundsSo = (SoundsSO)currentTarget;
                if (soundsSo == null)
                    continue;

                string[] names = Enum.GetNames(typeof(SoundType));
                SoundList[] soundList = soundsSo.sounds;

                bool differentSize = soundList == null || names.Length != soundList.Length;
                bool needsSync = differentSize;

                if (!needsSync && soundList != null)
                {
                    for (int i = 0; i < soundList.Length; i++)
                    {
                        if (soundList[i].name != names[i])
                        {
                            needsSync = true;
                            break;
                        }
                    }
                }

                if (!needsSync && soundList != null)
                {
                    for (int i = 0; i < soundList.Length; i++)
                    {
                        if (soundList[i].volume == 0)
                        {
                            needsSync = true;
                            break;
                        }
                    }
                }

                if (!needsSync)
                    continue;

                Undo.RecordObject(soundsSo, "Sync SoundsSO");

                if (soundList == null)
                    soundList = Array.Empty<SoundList>();

                Dictionary<string, SoundList> sounds = new();
                if (differentSize)
                {
                    for (int i = 0; i < soundList.Length; ++i)
                    {
                        if (!string.IsNullOrEmpty(soundList[i].name) && !sounds.ContainsKey(soundList[i].name))
                            sounds.Add(soundList[i].name, soundList[i]);
                    }
                }

                Array.Resize(ref soundList, names.Length);
                for (int i = 0; i < soundList.Length; i++)
                {
                    string currentName = names[i];
                    soundList[i].name = currentName;
                    if (soundList[i].volume == 0) soundList[i].volume = 1;

                    if (differentSize)
                    {
                        if (sounds.TryGetValue(currentName, out SoundList current))
                        {
                            UpdateElement(ref soundList[i], current.volume, current.sounds, current.mixer);
                        }
                        else
                        {
                            UpdateElement(ref soundList[i], 1, Array.Empty<AudioClip>(), null);
                        }

                        static void UpdateElement(ref SoundList element, float volume, AudioClip[] sounds, AudioMixerGroup mixer)
                        {
                            element.volume = volume;
                            element.sounds = sounds;
                            element.mixer = mixer;
                        }
                    }
                }

                soundsSo.sounds = soundList;
                EditorUtility.SetDirty(soundsSo);
            }
        }
    }

#endif