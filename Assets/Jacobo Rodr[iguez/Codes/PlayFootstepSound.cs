using UnityEngine;
using FMODUnity;

public class PlayFootstepSound : MonoBehaviour
{
    [Header("Tags")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string softGroundTag = "SoftGround";
    [SerializeField] private string hardGroundTag = "HardGround";

    [Header("Audio (FMOD)")]
    [SerializeField] private EventReference footstepEvent;
    [SerializeField] private string terrainParameterName = "Terrain";

    private float terrainParameterValue = 0f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        UpdateFootstepTypeFromCollision(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        UpdateFootstepTypeFromCollision(collision);
    }

    private void UpdateFootstepTypeFromCollision(Collision2D collision)
    {
        if (!CompareTag(playerTag))
            return;

        GameObject other = collision.collider != null ? collision.collider.gameObject : null;
        if (other == null)
            return;

        if (other.CompareTag(softGroundTag))
        {
            terrainParameterValue = 0f;
        }
        else if (other.CompareTag(hardGroundTag))
        {
            terrainParameterValue = 1f;
        }
    }

    // Call this from an Animation Event when the foot hits the ground.
    public void PlayFootstep()
    {
        if (AudioManager.instance == null)
            return;

        EventReference targetEvent = !footstepEvent.IsNull
            ? footstepEvent
            : (FMODEvents.instance != null ? FMODEvents.instance.FootstepsSoundEvent : default);

        if (!targetEvent.IsNull)
        {
            var instance = AudioManager.instance.CreateEventInstance(targetEvent, transform.position);
            if (instance.isValid())
            {
                if (!string.IsNullOrEmpty(terrainParameterName))
                    instance.setParameterByName(terrainParameterName, terrainParameterValue);

                instance.start();
                instance.release();
            }
        }
    }
}
