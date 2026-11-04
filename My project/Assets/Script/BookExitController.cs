using UnityEngine;
using UnityEngine.InputSystem;

public class BookExitController : MonoBehaviour
{
    public Animator bookAnimator;
    public Material fullscreenMaterial;

    [Header("Effects")]
    public ParticleSystem exitParticleSystem; // 👈 퇴장 클릭 시 재생할 파티클 시스템 연결

    public MonoBehaviour cameraController;

    public int effectStartFrame = 35;
    public float effectDuration = 3f;

    // 퇴장용 책 오브젝트의 콜라이더 이름 (필요시 인스펙터나 코드에서 수정 가능)
    public string targetBookColliderName = "book_exit";

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

        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
            return;

        if (Camera.main == null)
            return;

        Ray ray = Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue());
        RaycastHit[] hits = Physics.RaycastAll(ray, 1000f);

        foreach (RaycastHit hit in hits)
        {
            bool clickedBook =
                hit.collider.name == targetBookColliderName ||
                (bookAnimator != null && hit.collider.transform.IsChildOf(bookAnimator.transform));

            if (!clickedBook)
                continue;

            Debug.Log("퇴장용 책 클릭됨: 종료 연출 시작");

            // 👈 책이 클릭되는 순간 파티클 재생
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

            return;
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
        float strength = t * t * t; // 3제곱 이징 완만함 적용

        // 화면 소용돌이 왜곡 적용
        fullscreenMaterial.SetFloat("_Zoom", Mathf.Lerp(1f, 0.18f, strength));
        fullscreenMaterial.SetFloat("_TwirlStrength", Mathf.Lerp(0f, 12f, strength));

        // 연출 시간이 끝났을 때 -> 확실하게 게임 종료!
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