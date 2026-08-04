using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private bool hasAnyAnimator;
    private string maskTriggerName = "mask";

    public bool HasAnyAnimator => hasAnyAnimator;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        hasAnyAnimator = animator != null;
        if (!hasAnyAnimator)
        {
            Animator[] childAnimators = GetComponentsInChildren<Animator>(true);
            for (int i = 0; i < childAnimators.Length; i++)
            {
                if (childAnimators[i] != null)
                {
                    hasAnyAnimator = true;
                    break;
                }
            }
        }
    }

    public void SetMaskTriggerName(string triggerName)
    {
        maskTriggerName = triggerName;
    }

    public void ApplyFlipX(bool flip)
    {
        if (spriteRenderer != null)
            spriteRenderer.flipX = flip;

        SpriteRenderer[] children = GetComponentsInChildren<SpriteRenderer>();
        for (int i = 0; i < children.Length; i++)
        {
            SpriteRenderer sr = children[i];
            if (sr == null || sr == spriteRenderer) continue;
            sr.flipX = flip;
        }
    }

    public void TriggerMoveAnimation()
    {
        if (animator != null)
            animator.SetTrigger("move");

        Animator[] childAnimators = GetComponentsInChildren<Animator>();
        for (int i = 0; i < childAnimators.Length; i++)
        {
            Animator childAnimator = childAnimators[i];
            if (childAnimator == null || childAnimator == animator) continue;
            childAnimator.SetTrigger("move");
        }
    }

    public void TriggerMaskAnimation()
    {
        if (animator != null)
            animator.SetTrigger(maskTriggerName);
    }

    public void SetChildrenSpriteRenderersEnabled(bool enabled)
    {
        SpriteRenderer[] children = GetComponentsInChildren<SpriteRenderer>(true);
        for (int i = 0; i < children.Length; i++)
        {
            SpriteRenderer sr = children[i];
            if (sr == null || sr == spriteRenderer) continue;
            sr.enabled = enabled;
        }
    }
}