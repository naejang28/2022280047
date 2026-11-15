using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class BookOpenSceneController : MonoBehaviour
{
    [Header("애니메이션 & 쉐이더 설정")]
    public Animator doorAnimator;
    public Material fullscreenMaterial;
    public ParticleSystem doorParticle; // 문 열릴 때 터지는 이펙트 (선택사항)

    [Header("플레이어 제어")]
    public MonoBehaviour cameraController; // 마우스 시점 카메라 제어 스크립트
    public MonoBehaviour playerController; // 플레이어 이동 제어 스크립트

    [Header("연출 타이밍")]
    public int effectStartFrame = 35; // 애니메이션 중 쉐이더 효과가 시작될 프레임
    public float effectDuration = 3f;  // 쉐이더 효과 지속 시간

    private bool playingEffect = false;
    private float timer = 0f;
    private bool isInsideZone = false; // 플레이어가 존(Zone) 콜라이더 안에 있는지 여부

    void Start()
    {
        // 시작할 때 셰이더 값 초기화
        if (fullscreenMaterial != null)
        {
            fullscreenMaterial.SetFloat("_Zoom", 1f);
            fullscreenMaterial.SetFloat("_TwirlStrength", 0f);
        }
        playingEffect = false;
    }

    // 💡 플레이어가 존(Trigger Collider) 영역 안으로 들어왔을 때
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.name.Contains("Player"))
        {
            isInsideZone = true;
            Debug.Log("존(Zone) 진입: E키 또는 마우스 클릭으로 상호작용 가능");
        }
    }

    // 💡 플레이어가 존(Trigger Collider) 영역을 벗어났을 때
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.name.Contains("Player"))
        {
            isInsideZone = false;
            Debug.Log("존(Zone) 이탈: 상호작용 불가");
        }
    }

    void Update()
    {
        if (playingEffect)
            return;

        // 존 안에 없을 때는 입력을 받지 않음
        if (!isInsideZone)
            return;

        // 1. 키보드 'E' 키 입력 확인
        bool ePressed = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;

        // 2. 마우스 좌클릭 입력 확인
        bool mouseClicked = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;

        // 💡 존 안에 있는 상태에서 E키나 마우스 클릭 중 하나라도 누르면 바로 작동!
        if (ePressed || mouseClicked)
        {
            Debug.Log("존 안에서 상호작용 입력 감지: 문 열림 연출 시작");
            StartDoorEffect();
        }
    }

    // 연출 시작 공통 로직 (조작 제한 및 애니메이션/파티클 재생)
    void StartDoorEffect()
    {
        // 1. 플레이어 및 카메라 이동/시점 조작 제한 (못 움직이게 고정)
        SetControllersEnabled(false);

        // 2. 마우스 커서 설정
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        // 3. 파티클 재생
        if (doorParticle != null)
        {
            doorParticle.Play();
        }

        // 4. 문 애니메이션 트리거 실행
        if (doorAnimator != null)
        {
            doorAnimator.ResetTrigger("OpenBook");
            doorAnimator.SetTrigger("OpenBook");
        }

        // 5. 쉐이더 이펙트 타이머 가동 시작
        if (fullscreenMaterial != null)
        {
            timer = 0f;
            playingEffect = true;
        }
    }

    void LateUpdate()
    {
        if (!playingEffect || fullscreenMaterial == null || doorAnimator == null)
            return;

        AnimatorStateInfo state = doorAnimator.GetCurrentAnimatorStateInfo(0);

        // 애니메이션 상태 이름 확인 (문 애니메이션 클립 이름에 맞게 수정 필요, 예: "DoorOpen")
        if (!state.IsName("BookOpen"))
            return;

        float startTime = effectStartFrame / 30f;

        if (state.normalizedTime < startTime / state.length)
            return;

        timer += Time.deltaTime;

        float t = Mathf.Clamp01(timer / effectDuration);

        // 초반엔 평범하게 시작하다가 끝부분에서 강해지는 이징 커브
        float strength = Mathf.Pow(t, 4f);

        fullscreenMaterial.SetFloat(
          "_Zoom",
          Mathf.Lerp(1f, 0.1f, strength)
        );

        fullscreenMaterial.SetFloat(
          "_TwirlStrength",
          Mathf.Lerp(0f, 1.75f, strength)
        );

        // 연출 시간이 모두 끝나면 씬 이동
        if (t >= 1f)
        {
            playingEffect = false;

            // 쉐이더 값 리셋 (씬 전환 직전 깜빡임 방지)
            fullscreenMaterial.SetFloat("_Zoom", 1f);
            fullscreenMaterial.SetFloat("_TwirlStrength", 0f);

        }
    }

    // 컨트롤러 활성화/비활성화 일괄 처리 함수
    void SetControllersEnabled(bool isEnabled)
    {
        if (cameraController != null)
        {
            cameraController.enabled = isEnabled;
        }

        if (playerController != null)
        {
            playerController.enabled = isEnabled;
        }
    }
}