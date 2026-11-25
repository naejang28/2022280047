using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class BookExitController : MonoBehaviour
{
    public Animator bookAnimator;
    public Material fullscreenMaterial;

    [Header("씬 이동 설정")]
    public string SceneName;   // 넘어갈 씬 이름

    [Header("Effects")]
    public ParticleSystem exitParticleSystem; // 퇴장 시 재생할 파티클 시스템

    public MonoBehaviour cameraController;

    public int effectStartFrame = 35;
    public float effectDuration = 3f;

    private bool playingEffect = false;
    private float timer = 0f;
    private bool isInsideZone = false; // 💡 플레이어가 존(Zone) 콜라이더 안에 있는지 여부



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

    // 💡 플레이어가 존(Trigger Collider) 영역 안으로 들어왔을 때
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") || other.name.Contains("Player"))
        {
            isInsideZone = true;
            Debug.Log("책 존(Zone) 진입: E키 또는 마우스 클릭으로 상호작용 가능");
        }
    }

    // 💡 플레이어가 존(Trigger Collider) 영역을 벗어났을 때
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") || other.name.Contains("Player"))
        {
            isInsideZone = false;
            Debug.Log("책 존(Zone) 이탈: 상호작용 불가");
        }
    }

    void Update()
    {
        if (playingEffect)
            return;

        // 💡 존 안에 없을 때는 입력을 받지 않음
        if (!isInsideZone)
            return;

        // 1. 키보드 'E' 키 입력 확인
        bool ePressed = Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame;

        // 2. 마우스 좌클릭 입력 확인
        bool mouseClicked = Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame;

        // 💡 존 안에 있는 상태에서 E키나 마우스 클릭 중 하나라도 누르면 바로 종료 연출 시작!
        if (ePressed || mouseClicked)
        {
            Debug.Log("존 안에서 상호작용 입력 감지: 종료 연출 시작");
            StartExitEffect();
        }
    }

    // 연출 시작 공통 로직 함수
    void StartExitEffect()
    {
        if (exitParticleSystem != null)
        {
            exitParticleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
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

        if (!state.IsName("BookOpen"))
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
            Debug.Log("연출 완료: 씬 이동 실행 -> " + SceneName);
            SceneFader.Go(SceneName);
        }
    }
}