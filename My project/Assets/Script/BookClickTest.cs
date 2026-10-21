using UnityEngine;
using UnityEngine.InputSystem;

public class BookClickTest : MonoBehaviour
{
    public Animator bookAnimator;
    public Material fullscreenMaterial;

    public MonoBehaviour cameraController;
    private bool cameraWasEnabled;

    public int effectStartFrame = 35;
    public float effectDuration = 3f;

    public Transform playerTransform;
    public Transform bookStartPoint;
    public Transform destinationPoint;
    public float cameraMoveDuration = 2f;

    private bool playingEffect;
    private bool cameraMoving;
    private float timer;
    private float cameraMoveTimer;

    private Vector3 moveStartPosition;
    private Quaternion moveStartRotation;

    void Start()
    {
        Debug.Log("Start 실행됨");

        Debug.Log("Player: " +
            (playerTransform != null ? playerTransform.name : "연결 안 됨"));

        Debug.Log("BookStartPoint: " +
            (bookStartPoint != null ? bookStartPoint.name : "연결 안 됨"));

        if (playerTransform != null && bookStartPoint != null)
        {
            playerTransform.SetPositionAndRotation(
                bookStartPoint.position,
                bookStartPoint.rotation
            );

            Debug.Log("Player 이동 완료: " + playerTransform.position);
        }

        if (cameraController != null)
        {
            cameraController.enabled = false;
        }
    }

    void Update()
    {
        // 효과 종료 후 지정 위치로 플레이어 이동
        if (cameraMoving)
        {
            if (playerTransform == null ||
                destinationPoint == null)
            {
                cameraMoving = false;
                RestoreCameraControl();
                return;
            }

            cameraMoveTimer += Time.deltaTime;

            float t = cameraMoveDuration <= 0f
                ? 1f
                : Mathf.Clamp01(
                    cameraMoveTimer / cameraMoveDuration
                  );

            // 부드럽게 이동
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

        // 연출 중에는 책 클릭을 다시 받지 않음
        if (playingEffect)
            return;

        if (Mouse.current == null ||
            !Mouse.current.leftButton.wasPressedThisFrame)
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
                hit.collider.transform.parent?.name ==
                    "book_animation";

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

                if (cameraController != null)
                {
                    cameraWasEnabled =
                        cameraController.enabled;

                    cameraController.enabled = false;
                }
            }

            return;
        }
    }

    void LateUpdate()

    {
        if (!playingEffect ||
            fullscreenMaterial == null ||
            bookAnimator == null)
            return;

        AnimatorStateInfo state =
            bookAnimator.GetCurrentAnimatorStateInfo(0);
        Debug.Log("현재 애니메이션: " + state.fullPathHash);
        Debug.Log("애니메이션 이름 확인: " +
            state.IsName("Take 001_copy"));
        if (!state.IsName("Take 001_copy"))
            return;

        float startTime = effectStartFrame / 30f;

        if (state.normalizedTime <
            startTime / state.length)
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
        if (cameraController != null)
        {
            cameraController.enabled = true;
        }
    }
}