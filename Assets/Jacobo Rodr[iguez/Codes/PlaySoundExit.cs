using UnityEngine;
using FMODUnity;

public class PlaySoundExit : StateMachineBehaviour
{
    [SerializeField] private EventReference soundEvent;
    
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (AudioManager.instance == null)
            return;

        if (!soundEvent.IsNull)
        {
        AudioManager.instance.PlayOneShot(soundEvent, animator.transform.position);
        }
        else
        {
            Debug.LogWarning("Sound event is null for PlaySoundExit on state: " + stateInfo.fullPathHash);
        }
}
}

