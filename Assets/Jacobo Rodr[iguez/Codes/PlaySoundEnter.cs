using UnityEngine;
using FMODUnity;

public class PlaySoundEnter : StateMachineBehaviour
{
    [SerializeField] private EventReference soundEvent;
    
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (AudioManager.instance == null)
            return;

        if (!soundEvent.IsNull)
        {
            AudioManager.instance.PlayOneShot(soundEvent, animator.transform.position);
        }
        else
        {
            Debug.LogWarning("Sound event is null for PlaySoundEnter on state: " + stateInfo.fullPathHash);
        }
    }
}
