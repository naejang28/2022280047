using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class BookClickTest : MonoBehaviour
{
    public Animator bookAnimator;
    public Material fullscreenMaterial;
    public ParticleSystem bookMagicParticle; // 👈 마법 파티클 연결용
    public MonoBehaviour cameraController;
    public MonoBehaviour playerController;

    // 💡 [추가] 통째로 꺼버릴 책 오브젝트 (또는 책 컴포넌트가 붙어 있는 GameObject)
    public GameObject bookObject;

    private BookOutlineController outlineController;

    public int effectStartFrame = 35;
    public float effectDuration = 3f;

    public Transform playerTransform;
    public Transform bookStartPoint;
    public Transform destinationPoint;
    public float cameraMoveDuration = 2f;

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
        Debug.Log("=== Start 실행됨 / Skip Start 체크 상태: " + skipStart + " ===");

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

            // 💡 스킵으로 시작할 때 책 통째로 끄기
            HideBook();
        }
        else
        {
            if (playerTransform != null && bookStartPoint != null)
            {
                playerTransform.SetPositionAndRotation(
                  bookStartPoint.position,
                  bookStartPoint.rotation
                );
            }

            SetControllersEnabled(false);
            SetCursorState(true);
        }
    }

    void Update()
    {
        if (!playingEffect && !cameraMoving && !skipStart && (cameraController == null || !cameraController.enabled))
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

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

        if (playingEffect)
            return;

        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame)
            return;

        if (Camera.main == null)
            return;

        Ray ray = Camera.main.ScreenPointToRay(
          Mouse.current.position.ReadValue()
        );

        RaycastHit[] hits = Physics.RaycastAll(ray, 1000f);

        foreach (RaycastHit hit in hits)
        {
            bool clickedBook =
              hit.collider.name == "book_animation" ||
              hit.collider.transform.IsChildOf(
                bookAnimator != null
                  ? bookAnimator.transform
                  : transform
              ) ||
              hit.collider.transform.parent?.name == "book_animation";

            if (!clickedBook)
                continue;

            if (bookAnimator != null)
            {
                bookAnimator.ResetTrigger("OpenBook");
                bookAnimator.SetTrigger("OpenBook");
            }

            if (fullscreenMaterial != null)
            {
                timer = 0f;
                playingEffect = true;
                SetControllersEnabled(false);
                SetCursorState(false);

                // 💡 책을 '클릭했을 때'만 파티클이 재생되도록 이 위치에 있어야 합니다!
                if (bookMagicParticle != null)
                {
                    bookMagicParticle.Play();
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
        float strength = t * t * t;

        fullscreenMaterial.SetFloat(
          "_Zoom",
          Mathf.Lerp(1f, 0.18f, strength)
        );

        fullscreenMaterial.SetFloat(
          "_TwirlStrength",
          Mathf.Lerp(0f, 12f, strength)
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
                cameraMoveDuration = 0.1f;
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

        // 💡 연출이 끝나고 도착 지점에 도달했을 때 책 통째로 끄기
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

    // 💡 책 오브젝트와 아웃라인을 안전하게 꺼버리는 함수
    void HideBook()
    {
        if (outlineController != null)
        {
            outlineController.enabled = false;
        }

        if (bookObject != null)
        {
            bookObject.SetActive(false); // 책 오브젝트 비활성화
        }
        else
        {
            // 만약 변수에 따로 안 넣었다면 이 스크립트가 붙은 오브젝트 자체를 끄기
            gameObject.SetActive(false);
        }
    }
}