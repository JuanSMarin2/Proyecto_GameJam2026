using UnityEngine;
using FMODUnity;
using FMOD.Studio;
using System.Collections.Generic;
public class AudioManager : MonoBehaviour
{
   public static AudioManager instance { get; private set; }

private List<EventInstance> activeEvents = new List<EventInstance>();
    //Propiedad singleton
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.Log("Mas de un AudioManager en escena, destruyendo el más nuevo.");
            Destroy(gameObject);
        }
    }


    public void PlayOneShot(EventReference soundEvent, Vector3 position)
    {
        RuntimeManager.PlayOneShot(soundEvent, position);
    }

    public EventInstance CreateEventInstance(EventReference soundEventReference)
    {
        EventInstance eventInstance = RuntimeManager.CreateInstance(soundEventReference);
        activeEvents.Add(eventInstance);
        return eventInstance;
    }

    private void CleanUp()
    {
        foreach (EventInstance eventInstance in activeEvents)
        {
            eventInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            eventInstance.release();
        }
    }
    
    private void OnDestroy()
    {
        CleanUp();
    }
}
