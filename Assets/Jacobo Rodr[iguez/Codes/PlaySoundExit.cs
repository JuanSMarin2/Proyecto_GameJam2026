using UnityEngine;
using FMODUnity;

public class PlaySoundExit : StateMachineBehaviour
{
    [SerializeField] private EventReference soundEvent;
    [SerializeField] private bool playOnce;

    private bool played;

    
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (AudioManager.instance == null)
            return;

        if (playOnce && played)
            return;

        played = true;

        if (!soundEvent.IsNull && playOnce && !played)
        {
            AudioManager.instance.PlayOneShot(soundEvent, animator.transform.position);
            played = true;
        }
        else if (!soundEvent.IsNull && !playOnce)
        {
        AudioManager.instance.PlayOneShot(soundEvent, animator.transform.position);
        }
        else
        {
            Debug.LogWarning("Sound event is null for PlaySoundExit on state: " + stateInfo.fullPathHash);
        }
}
}

