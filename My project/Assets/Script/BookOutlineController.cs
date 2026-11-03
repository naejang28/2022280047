using UnityEngine;
using UnityEngine.InputSystem;

public class BookOutlineController : MonoBehaviour
{
    private Outline outlineComponent;
    private bool isOutlineActive = false;

    void Start()
    {
        // 1. 꺼져 있는 아웃라인 컴포넌트 안전하게 가져오기
        outlineComponent = GetComponentInChildren<Outline>(true);

        if (outlineComponent != null)
        {
            outlineComponent.enabled = false;
            isOutlineActive = false;
        }
    }

    void Update()
    {
        if (Camera.main == null || Mouse.current == null || outlineComponent == null)
            return;

        // 마우스 포인터 위치에서 레이캐스트 발사
        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit[] hits = Physics.RaycastAll(ray, 1000f);

        bool isHovering = false;

        foreach (RaycastHit hit in hits)
        {
            // 현재 책 오브젝트 또는 그 자식을 정확히 가리키고 있는지 확인
            if (hit.collider.transform == transform || hit.collider.transform.IsChildOf(transform))
            {
                isHovering = true;
                break;
            }
        }

        // 상태가 바뀔 때만 아웃라인을 켜고 꺼서 불필요한 부하 및 물리 꼬임 방지
        if (isHovering != isOutlineActive)
        {
            isOutlineActive = isHovering;
            outlineComponent.enabled = isOutlineActive;
        }
    }
}