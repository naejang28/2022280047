using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BookClickTest : MonoBehaviour
{
    public Animator bookAnimator;
    public Material fullscreenMaterial;
    public ParticleSystem bookMagicParticle;
    public MonoBehaviour cameraController;
    public MonoBehaviour playerController;

    public GameObject bookObject;

    private BookOutlineController outlineController;

    public int effectStartFrame = 35;
    public float effectDuration = 3f;

    public Transform playerTransform;
    public Transform bookStartPoint;
    public Transform destinationPoint;
    public float cameraMoveDuration = 0.1f; // 💡 원본 속도 유지

    [Header("테스트 설정")]
    public bool skipStart = false;

    private bool playingEffect;
    private bool cameraMoving;
    private float timer;
    private float cameraMoveTimer;

    private Vector3 moveStartPosition;
    private Quaternion moveStartRotation;

    void Start()
    {
        Debug.Log("=== 씬 이동 후 자동 실행 ===");

        outlineController = GetComponentInChildren<BookOutlineController>(true);

        SetControllersEnabled(false);

        if (skipStart)
        {
            if (playerTransform != null && destinationPoint != null)
            {
                playerTransform.SetPositionAndRotation(
                  destinationPoint.position,
                  destinationPoint.rotation
                );
            }

            SetControllersEnabled(true);
            SetCursorState(false);
            HideBook();
        }
        else
        {
            // 💡 시작 위치 설정
            if (playerTransform != null && bookStartPoint != null)
            {
                playerTransform.SetPositionAndRotation(
                  bookStartPoint.position,
                  bookStartPoint.rotation
                );
            }

            SetControllersEnabled(false);
            SetCursorState(false);

            // 💡 씬이 열리자마자 마우스 클릭 없이 바로 애니메이션과 효과 자동 시작!
            if (bookAnimator != null)
            {
                bookAnimator.ResetTrigger("OpenBook");
                bookAnimator.SetTrigger("OpenBook");
            }

            if (fullscreenMaterial != null)
            {
                timer = 0f;
                playingEffect = true;

                if (bookMagicParticle != null)
                {
                    bookMagicParticle.Play();
                }
            }
        }
    }

    void Update()
    {
        // 💡 수동 클릭 감지 코드는 제거됨 (씬 로드 시 자동 실행되므로)

        if (cameraMoving)
        {
            if (playerTransform == null || destinationPoint == null)
            {
                cameraMoving = false;
                RestoreCameraControl();
                return;
            }

            cameraMoveTimer += Time.deltaTime;

            float t = cameraMoveDuration <= 0f
              ? 1f
              : Mathf.Clamp01(cameraMoveTimer / cameraMoveDuration);

            float smoothT = t * t * (3f - 2f * t);

            playerTransform.SetPositionAndRotation(
              Vector3.Lerp(
                moveStartPosition,
                destinationPoint.position,
                smoothT
              ),
              Quaternion.Slerp(
                moveStartRotation,
                destinationPoint.rotation,
                smoothT
              )
            );

            if (t >= 1f)
            {
                cameraMoving = false;
                RestoreCameraControl();
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

        float startTime = effectStartFrame / 20f;

        if (state.normalizedTime < startTime / state.length)
            return;

        timer += Time.deltaTime;

        float t = Mathf.Clamp01(timer / effectDuration);

        // 💡 초반엔 평범하게 시작했다가 끝에서 확 치솟는 형태
        float strength = Mathf.Pow(t, 4f);
        // 또는 그냥 곱셈으로 쓰셔도 됩니다: float strength = t * t * t * t * t;

        fullscreenMaterial.SetFloat(
          "_Zoom",
          Mathf.Lerp(1f, 0.1f, strength)
        );

        fullscreenMaterial.SetFloat(
          "_TwirlStrength",
          Mathf.Lerp(0f, 1.75f, strength)
        );

        if (t >= 1f)
        {
            playingEffect = false;

            fullscreenMaterial.SetFloat("_Zoom", 1f);
            fullscreenMaterial.SetFloat("_TwirlStrength", 0f);

            if (playerTransform != null && destinationPoint != null)
            {
                moveStartPosition = playerTransform.position;
                moveStartRotation = playerTransform.rotation;

                cameraMoveTimer = 0f;
                cameraMoveDuration = 0.1f; // 💡 원본 이동 모션 속도 유지
                cameraMoving = true;
            }
            else
            {
                RestoreCameraControl();
            }
        }
    }

    void RestoreCameraControl()
    {
        SetControllersEnabled(true);
        SetCursorState(false);
        HideBook();
    }

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

    void SetCursorState(bool isVisible)
    {
        if (isVisible)
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    void HideBook()
    {
        if (outlineController != null)
        {
            outlineController.enabled = false;
        }

        if (bookObject != null)
        {
            bookObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}