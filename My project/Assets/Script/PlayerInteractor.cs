using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteractor : MonoBehaviour
{
    public FirstPersonMover mover;     // Player의 FirstPersonMover

    private Exhibit nearby;            // 지금 가까이 있는 작품
    private GameObject openPanel;      // 지금 열려 있는 패널


    private void OnTriggerEnter(Collider other)
    {
        Exhibit e = other.GetComponent<Exhibit>();
        if (e == null) return;
        nearby = e;
        nearby.ShowPrompt(true);
    }

    private void OnTriggerExit(Collider other)
    {
        Exhibit e = other.GetComponent<Exhibit>();
        if (e == null || e != nearby) return;
        nearby.ShowPrompt(false);
        nearby = null;
    }

    public void OnInteract(InputValue value)
    {
        if (!value.isPressed) return;

        // 이미 열려 있으면 닫기
        if (openPanel != null)
        {
            openPanel.SetActive(false);
            openPanel = null;
            if (mover != null) mover.canMove = true;
            return;
        }

        if (nearby == null) return;        // 가까운 작품이 없으면 무시

        // 🛡️ [안전장치 추가] 패널이 연결되어 있는지 확인
        if (nearby.panel == null)
        {
            Debug.LogWarning($"[{nearby.gameObject.name}] 전시물에 연결된 패널(Panel)이 없습니다! 인스펙터창을 확인해주세요.", nearby.gameObject);
            return;
        }

        openPanel = nearby.panel;
        openPanel.SetActive(true);

        if (mover != null) mover.canMove = false;
    }
}