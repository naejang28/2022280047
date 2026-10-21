
using UnityEngine;

public class PopupBookProximity : MonoBehaviour
{
    public Animator bookAnimator;
    public Animator popupAnimator;
    public Transform player;
    public float openDistance = 20f;
    public Transform distancePoint;
    private bool isOpen;

    void Update()
    {
        if (player == null) return;

        Vector3 targetPosition = distancePoint != null
            ? distancePoint.position
            : transform.position;

        float distance = Vector3.Distance(
            player.position,
            targetPosition
        );

        bool shouldOpen = distance <= openDistance;

        if (shouldOpen == isOpen) return;

        isOpen = shouldOpen;

        PlayBookAnimation(bookAnimator, isOpen);
        PlayBookAnimation(popupAnimator, isOpen);
    }

    void PlayBookAnimation(Animator anim, bool open)
    {
        if (anim == null) return;

        anim.speed = 1f;
        anim.SetBool("IsOpen", open);
    }
}
