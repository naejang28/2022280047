using UnityEngine;

public class Exhibit : MonoBehaviour
{
    public GameObject panel;     // 이 작품의 상세 보기 패널
    public GameObject prompt;    // 가까이 가면 켜지는 E 표시

    private Transform playerTransform;
    private Outline outlineComponent; // 아웃라인 에셋 컴포넌트

    void Start()
    {
        // 1. 씬에서 플레이어 태그로 플레이어 위치 미리 찾아두기
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }

        // 2. 오브젝트나 자식에 붙은 아웃라인 컴포넌트 가져오기
        outlineComponent = GetComponentInChildren<Outline>(true);
        if (outlineComponent != null)
        {
            outlineComponent.enabled = false; // 처음에는 꺼두기
        }
    }

    void LateUpdate()
    {
        // 프롬프트가 켜져 있고 플레이어가 있을 때만 실행
        if (prompt != null && prompt.activeSelf && playerTransform != null)
        {
            // 플레이어 방향 벡터를 구한 뒤, y값(높이 차이)을 0으로 만들어 위아래 회전 원천 차단
            Vector3 direction = playerTransform.position - prompt.transform.position;
            direction.y = 0;

            if (direction != Vector3.zero)
            {
                // 수평 방향으로 회전시킨 뒤, 뒤집혀 나오는 텍스트를 바로잡기 위해 180도 회전 추가
                prompt.transform.rotation = Quaternion.LookRotation(direction);
                prompt.transform.Rotate(0, 180, 0); // 👈 이 줄이 좌우 반전을 바르게 잡아줍니다!
            }
        }
    }

    public void ShowPrompt(bool on)
    {
        if (prompt != null)
        {
            prompt.SetActive(on);
        }

        // 가까이 갔을 때 테두리(아웃라인) 켜고 끄기
        if (outlineComponent != null)
        {
            outlineComponent.enabled = on;
        }
    }
}