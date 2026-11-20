using UnityEngine;
using FMODUnity;

public class PlaySoundEnter : StateMachineBehaviour
{
    [SerializeField] private EventReference soundEvent;
    [SerializeField] private SoundType sound;
    [SerializeField, Range(0, 1)] private float volume = 1;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (AudioManager.instance == null)
            return;

        if (!soundEvent.IsNull)
        {
            AudioManager.instance.PlayOneShot(soundEvent, animator.transform.position);
        }
        else if (FMODEvents.instance != null)
        {
            EventReference evt = FMODEvents.instance.GetEventByType(sound);
            if (!evt.IsNull)
            {
                AudioManager.instance.PlayOneShot(evt, animator.transform.position);
            }
        }
    }
}
