using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FirstPersonMover : MonoBehaviour
{
    [Header("이동")]
    public float walkSpeed = 4f;
    public float runSpeed = 7f;
    public float crouchSpeed = 2f;
    public float lookSpeed = 0.1f;
    public Transform cam;
    public bool canMove = true;

    [Header("키 설정 (Input Actions 불필요)")]
    public Key runKey = Key.LeftShift;
    public Key crouchKey = Key.LeftCtrl;
    public Key jumpKey = Key.Space;

    [Header("가속 / 감속")]
    public float acceleration = 12f;
    public float deceleration = 14f;
    public float airControl = 0.3f;

    [Header("점프 / 중력")]
    public float jumpHeight = 1.0f;
    public float gravity = -20f;

    [Header("앉기")]
    public float standHeight = 1.8f;
    public float crouchHeight = 1.0f;
    public float crouchTransitionSpeed = 8f;
    public float standCamY = 1.6f;
    public float crouchCamY = 0.9f;

    [Header("흔들림 - 걷기")]
    public bool enableBob = true;
    public float walkBobFrequency = 1.6f;   // 걸음 주기 (클수록 빠르게 흔들림)
    public float walkBobY = 0.025f;         // 상하 크기
    public float walkBobX = 0.015f;         // 좌우 크기
    public float walkBobRoll = 0.4f;        // 좌우 기울기(도)

    [Header("흔들림 - 달리기")]
    public float runBobFrequency = 1.9f;
    public float runBobY = 0.05f;
    public float runBobX = 0.03f;
    public float runBobRoll = 0.9f;

    [Header("흔들림 - 공통")]
    public float bobBlendSpeed = 10f;       // 흔들림 크기가 바뀌는 부드러움
    public float bobReturnSpeed = 8f;       // 멈췄을 때 원위치로 돌아오는 속도

    [Header("달리기 시야각(FOV)")]
    public Camera cameraComponent;          // 비워두면 cam에서 자동 검색
    public float walkFov = 60f;
    public float runFov = 68f;
    public float fovSpeed = 8f;

    [Header("점프 보정")]
    public float coyoteTime = 0.1f;
    public float jumpBufferTime = 0.1f;

    [Header("착지 충격")]
    public float landDipMax = 0.08f;
    public float landDipRecovery = 10f;

    private CharacterController cc;
    private Vector2 moveInput;
    private Vector2 lookInput;
    private float pitch;

    private Vector3 horizontalVelocity;
    private float verticalVelocity;
    private float coyoteTimer;
    private float jumpBufferTimer;

    private bool crouchHeld;
    private bool runHeld;
    private bool isRunning;

    private float bobTimer;
    private float bobY, bobX, bobRoll;      // 현재 적용 중인 흔들림 값
    private float currentBaseCamY;

    private bool wasGrounded;
    private float lastFallSpeed;
    private float landDip;

    private void Awake()
    {
        cc = GetComponent<CharacterController>();
        cc.height = standHeight;
        cc.center = new Vector3(0f, standHeight * 0.5f, 0f);
        currentBaseCamY = standCamY;

        if (cameraComponent == null && cam != null)
            cameraComponent = cam.GetComponent<Camera>();
        if (cameraComponent != null)
            walkFov = cameraComponent.fieldOfView; // 현재 FOV를 걷기 기준으로 사용
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (!canMove) return;

        ReadKeys();
        HandleLook();
        HandleCrouch();
        HandleMove();
        HandleBob();
        HandleFov();
    }

    void ReadKeys()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        crouchHeld = kb[crouchKey].isPressed;
        runHeld = kb[runKey].isPressed;
        if (kb[jumpKey].wasPressedThisFrame) jumpBufferTimer = jumpBufferTime;
    }

    void HandleLook()
    {
        transform.Rotate(0f, lookInput.x * lookSpeed, 0f);
        pitch = Mathf.Clamp(pitch - lookInput.y * lookSpeed, -60f, 60f);
        // 회전은 HandleBob에서 roll과 함께 적용
    }

    void HandleCrouch()
    {
        float targetHeight = crouchHeld ? crouchHeight : standHeight;

        if (!crouchHeld && cc.height < standHeight - 0.01f)
        {
            float radius = cc.radius * 0.95f;
            Vector3 origin = transform.position + Vector3.up * cc.height;
            float checkDist = standHeight - cc.height;
            if (Physics.SphereCast(origin - Vector3.up * radius, radius,
                Vector3.up, out _, checkDist, ~0, QueryTriggerInteraction.Ignore))
            {
                targetHeight = cc.height;
            }
        }

        cc.height = Mathf.Lerp(cc.height, targetHeight, Time.deltaTime * crouchTransitionSpeed);
        cc.center = new Vector3(0f, cc.height * 0.5f, 0f);

        float targetCamY = crouchHeld ? crouchCamY : standCamY;
        currentBaseCamY = Mathf.Lerp(currentBaseCamY, targetCamY, Time.deltaTime * crouchTransitionSpeed);
    }

    void HandleMove()
    {
        bool grounded = cc.isGrounded;

        // 코요테 타임 / 점프 버퍼
        if (grounded) coyoteTimer = coyoteTime;
        else coyoteTimer -= Time.deltaTime;
        jumpBufferTimer -= Time.deltaTime;

        if (grounded && verticalVelocity < 0f)
            verticalVelocity = -2f;

        if (jumpBufferTimer > 0f && coyoteTimer > 0f && !crouchHeld)
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            jumpBufferTimer = 0f;
            coyoteTimer = 0f;
        }

        verticalVelocity += gravity * Time.deltaTime;

        // 달리기: 앞으로 이동 중일 때만, 앉은 상태에서는 불가
        isRunning = runHeld && !crouchHeld && moveInput.y > 0.1f;

        float speed = crouchHeld ? crouchSpeed : (isRunning ? runSpeed : walkSpeed);
        Vector3 dir = transform.forward * moveInput.y + transform.right * moveInput.x;
        dir = Vector3.ClampMagnitude(dir, 1f);

        bool hasInput = dir.sqrMagnitude > 0.01f;
        float accel = hasInput ? acceleration : deceleration;
        if (!grounded) accel *= airControl;

        horizontalVelocity = Vector3.MoveTowards(
            horizontalVelocity, dir * speed, accel * Time.deltaTime);

        cc.Move((horizontalVelocity + Vector3.up * verticalVelocity) * Time.deltaTime);

        // 착지 충격 계산
        if (!wasGrounded && cc.isGrounded && lastFallSpeed < -4f)
            landDip = Mathf.Clamp(-lastFallSpeed * 0.01f, 0f, landDipMax);
        lastFallSpeed = verticalVelocity;
        wasGrounded = cc.isGrounded;
        landDip = Mathf.Lerp(landDip, 0f, Time.deltaTime * landDipRecovery);
    }

    void HandleBob()
    {
        float planarSpeed = new Vector2(horizontalVelocity.x, horizontalVelocity.z).magnitude;
        bool moving = enableBob && planarSpeed > 0.2f && cc.isGrounded;

        float targetY = 0f, targetX = 0f, targetRoll = 0f;

        if (moving)
        {
            float t = Mathf.InverseLerp(walkSpeed, runSpeed, planarSpeed); // 0=걷기, 1=달리기
            float freq = Mathf.Lerp(walkBobFrequency, runBobFrequency, t);
            float ampY = Mathf.Lerp(walkBobY, runBobY, t);
            float ampX = Mathf.Lerp(walkBobX, runBobX, t);
            float ampRoll = Mathf.Lerp(walkBobRoll, runBobRoll, t);

            // 실제 이동 거리에 비례해 걸음 진행 → 빠르면 걸음도 빨라짐
            bobTimer += planarSpeed * freq * Time.deltaTime;

            // 한 걸음(왼/오)마다 한 번씩 출렁임. abs로 발이 닿을 때 가장 낮게
            float step = Mathf.Sin(bobTimer * 2f);
            targetY = -Mathf.Abs(Mathf.Sin(bobTimer)) * ampY * 2f + ampY;
            targetX = Mathf.Cos(bobTimer) * ampX;
            targetRoll = Mathf.Cos(bobTimer) * ampRoll;
        }

        // 켜질 때 부드럽게 커지고, 멈출 때 부드럽게 0으로 복귀
        float blend = moving ? bobBlendSpeed : bobReturnSpeed;
        bobY = Mathf.Lerp(bobY, targetY, Time.deltaTime * blend);
        bobX = Mathf.Lerp(bobX, targetX, Time.deltaTime * blend);
        bobRoll = Mathf.Lerp(bobRoll, targetRoll, Time.deltaTime * blend);

        cam.localPosition = new Vector3(bobX, currentBaseCamY + bobY - landDip, 0f);
        cam.localRotation = Quaternion.Euler(pitch, 0f, bobRoll);
    }

    void HandleFov()
    {
        if (cameraComponent == null) return;
        float target = (isRunning && horizontalVelocity.sqrMagnitude > 1f) ? runFov : walkFov;
        cameraComponent.fieldOfView = Mathf.Lerp(
            cameraComponent.fieldOfView, target, Time.deltaTime * fovSpeed);
    }

    // --- Input System 메시지 (이동/시점만 사용) ---
    public void OnMove(InputValue value) { moveInput = value.Get<Vector2>(); }
    public void OnLook(InputValue value) { lookInput = value.Get<Vector2>(); }

    public void OnAttack(InputValue value)
    { if (canMove) Cursor.lockState = CursorLockMode.Locked; }
}