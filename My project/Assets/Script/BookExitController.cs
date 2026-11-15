using UnityEngine;
using UnityEngine.InputSystem;

public class BookExitController : MonoBehaviour
{
    public Animator bookAnimator;
    public Material fullscreenMaterial;

    [Header("Effects")]
    public ParticleSystem exitParticleSystem; // 퇴장 시 재생할 파티클 시스템

    public MonoBehaviour cameraController;

    public int effectStartFrame = 35;
    public float effectDuration = 3f;

    [Header("상호작용 설정")]
    public string targetBookColliderName = "book_exit";
    public float interactionDistance = 5f; // 💡 E 키로 상호작용 가능한 최대 거리 (필요시 조절 가능)

    private bool playingEffect;
    private float timer;

    void Start()
    {
        // 시작할 때 셰이더 초기화
        if (fullscreenMaterial != null)
        {
            fullscreenMaterial.SetFloat("_Zoom", 1f);
            fullscreenMaterial.SetFloat("_TwirlStrength", 0f);
        }

        playingEffect = false;
    }

    void Update()
    {
        if (playingEffect)
            return;

        if (Camera.main == null)
            return;

        // 1. 키보드 'E' 키를 눌렀을 때 -> 화면 중앙(플레이어가 바라보는 방향)에 책이 있는지 검사
        bool ePressed = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;
        if (ePressed)
        {
            // 화면 중앙(스크린 0.5, 0.5)에서 레이 발사
            Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
            RaycastHit[] hits = Physics.RaycastAll(ray, interactionDistance);

            foreach (RaycastHit hit in hits)
            {
                if (IsBookHit(hit))
                {
                    Debug.Log("책을 바라보고 E 키 입력됨: 종료 연출 시작");
                    StartExitEffect();
                    return;
                }
            }
        }

        // 2. 마우스 클릭 시 기존 레이캐스트 판정
        bool mouseClicked = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;
        if (mouseClicked)
        {
            Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
            RaycastHit[] hits = Physics.RaycastAll(ray, 1000f);

            foreach (RaycastHit hit in hits)
            {
                if (IsBookHit(hit))
                {
                    Debug.Log("퇴장용 책 클릭됨: 종료 연출 시작");
                    StartExitEffect();
                    return;
                }
            }
        }
    }

    // 책 오브젝트인지 판정하는 공통 함수
    bool IsBookHit(RaycastHit hit)
    {
        return hit.collider.name == targetBookColliderName ||
               (bookAnimator != null && hit.collider.transform.IsChildOf(bookAnimator.transform));
    }

    // 연출 시작 공통 로직 함수
    void StartExitEffect()
    {
        if (exitParticleSystem != null)
        {
            exitParticleSystem.Play();
        }

        if (bookAnimator != null)
        {
            bookAnimator.ResetTrigger("OpenBook");
            bookAnimator.SetTrigger("OpenBook");
        }

        if (fullscreenMaterial != null)
        {
            timer = 0f;
            playingEffect = true;

            if (cameraController != null)
            {
                cameraController.enabled = false;
            }
        }
    }

    void LateUpdate()
    {
        if (!playingEffect || fullscreenMaterial == null || bookAnimator == null)
            return;

        AnimatorStateInfo state = bookAnimator.GetCurrentAnimatorStateInfo(0);

        if (!state.IsName("Take 001_copy"))
            return;

        float startTime = effectStartFrame / 30f;

        if (state.normalizedTime < startTime / state.length)
            return;

        timer += Time.deltaTime;

        float t = Mathf.Clamp01(timer / effectDuration);
        float strength = Mathf.Pow(t, 4f);

        fullscreenMaterial.SetFloat(
          "_Zoom",
          Mathf.Lerp(1f, 0.1f, strength)
        );

        fullscreenMaterial.SetFloat(
          "_TwirlStrength",
          Mathf.Lerp(0f, 1.75f, strength)
        );

        // 연출 시간이 끝났을 때 -> 게임 종료
        if (t >= 1f)
        {
            playingEffect = false;

            fullscreenMaterial.SetFloat("_Zoom", 1f);
            fullscreenMaterial.SetFloat("_TwirlStrength", 0f);

            Debug.Log("퇴장 연출 완료: 게임 종료 실행");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false; // 에디터 정지
#else            
            Application.Quit(); // 빌드된 게임 종료
#endif
        }
    }
}