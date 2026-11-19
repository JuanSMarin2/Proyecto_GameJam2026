using UnityEngine;
using FMODUnity;

public class PlaySoundEnter : StateMachineBehaviour
{
    [SerializeField] private EventReference soundEvent;
    [SerializeField] private SoundType sound;
    [SerializeField, Range(0, 1)] private float volume = 1f;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (AudioManager.instance == null)
            return;

        EventReference targetEvent = !soundEvent.IsNull
            ? soundEvent
            : (FMODEvents.instance != null ? FMODEvents.instance.GetEventByType(sound) : default);

        if (!targetEvent.IsNull)
        {
            if (Mathf.Approximately(volume, 1f))
            {
                AudioManager.instance.PlayOneShot(targetEvent, animator.transform.position);
            }
            else
            {
                var eventInstance = AudioManager.instance.CreateEventInstance(targetEvent, animator.transform.position);
                if (eventInstance.isValid())
                {
                    eventInstance.setVolume(volume);
                    eventInstance.start();
                    eventInstance.release();
                }
            }
        }
    }
}
