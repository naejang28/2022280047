using UnityEngine;

public class PopupBookProximity : MonoBehaviour
{
    public Animator bookAnimator;
    public Animator popupAnimator;
    public Transform player;
    public float openDistance = 20f;
    public Transform distancePoint;
    private bool isOpen;

    [Header("책(Book) 애니메이션 속도 조절")]
    [Tooltip("책이 열릴 때의 재생 속도")]
    public float bookOpenSpeed = 1f;
    [Tooltip("책이 닫힐 때의 재생 속도")]
    public float bookCloseSpeed = 1f;

    [Header("팝업(Popup) 애니메이션 속도 조절")]
    [Tooltip("팝업이 열릴 때의 재생 속도")]
    public float popupOpenSpeed = 1f;
    [Tooltip("팝업이 닫힐 때의 재생 속도")]
    public float popupCloseSpeed = 1f;

    void Start()
    {
        // 게임 시작할 때 기본적으로 닫힌 상태로 시작하도록 세팅
        if (bookAnimator != null) bookAnimator.SetBool("IsOpen", false);
        if (popupAnimator != null) popupAnimator.SetBool("IsOpen", false);
    }

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

        // 열릴 때와 닫힐 때의 속도를 각각 분기하여 적용
        float currentBookSpeed = isOpen ? bookOpenSpeed : bookCloseSpeed;
        float currentPopupSpeed = isOpen ? popupOpenSpeed : popupCloseSpeed;

        PlayBookAnimation(bookAnimator, isOpen, currentBookSpeed);
        PlayBookAnimation(popupAnimator, isOpen, currentPopupSpeed);
    }

    void PlayBookAnimation(Animator anim, bool open, float speed)
    {
        if (anim == null) return;

        // 속도가 0이거나 음수여서 멈추는 현상 방지 (최소 속도 보장)
        if (speed <= 0.01f) speed = 0.01f;

        anim.speed = speed;
        anim.SetBool("IsOpen", open);
    }
}