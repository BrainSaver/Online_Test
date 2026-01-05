using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem; // ★ 필수

public class PlayerController : NetworkBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private VariableJoystick joystick;

    [Header("이동 설정")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("대시 설정")]
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1.0f;
    [SerializeField] private float doubleTapSpeed = 0.3f; // 더블탭 간격

    private Rigidbody2D rb;
    private Vector2 movementInput;
    private Vector2 lastMoveDirection = Vector2.right;

    // 더블탭 감지용
    private float lastTapTime = 0f;

    public bool IsDashing { get; private set; }
    public bool canMove = true;
    private bool canDash = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            var cam = Object.FindFirstObjectByType<CameraController>();
            if (cam != null) cam.SetTarget(transform);

            if (joystick == null) joystick = FindFirstObjectByType<VariableJoystick>();
        }
    }

    void Update()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, 0f);

        if (IsSpawned && !IsOwner) return;
        if (IsDashing) return;

        // 1. 이동 입력 (키보드)
        float inputX = 0;
        float inputY = 0;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) inputY = 1;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) inputY = -1;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) inputX = -1;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) inputX = 1;
        }

        // 2. 조이스틱 입력
        if (inputX == 0 && inputY == 0 && joystick != null)
        {
            inputX = joystick.Horizontal;
            inputY = joystick.Vertical;
        }

        // 방향 저장
        if (inputX != 0 || inputY != 0)
        {
            lastMoveDirection = new Vector2(inputX, inputY).normalized;
        }

        // 3. ★ 더블 탭 감지 (New Input System 방식)
        CheckDoubleTap();

        // 4. 이동 적용
        if (!IsDashing && canMove)
        {
            Vector2 rawInput = new Vector2(inputX, inputY);
            movementInput = Vector2.ClampMagnitude(rawInput, 1f);
        }
    }

    // ★ 터치와 마우스를 모두 감지하는 함수
    // ★ 수정된 함수: 기계적 오류(바운싱) 차단 기능 추가
    private void CheckDoubleTap()
    {
        bool tappedRightSide = false;

        // A. 모바일 터치 감지
        if (Touchscreen.current != null)
        {
            foreach (var touch in Touchscreen.current.touches)
            {
                if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
                {
                    Vector2 pos = touch.position.ReadValue();
                    if (pos.x > Screen.width * 0.5f)
                    {
                        tappedRightSide = true;
                    }
                }
            }
        }

        // B. PC 마우스 (테스트용)
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            if (mousePos.x > Screen.width * 0.5f)
            {
                tappedRightSide = true;
            }
        }

        // 더블탭 판정 로직
        if (tappedRightSide)
        {
            float timeSinceLastTap = Time.time - lastTapTime;

            // [핵심 수정] 
            // 0.05초보다 빠르게 들어온 터치는 손 떨림이나 센서 오류로 보고 무시합니다.
            // (사람은 0.05초 만에 두 번 누를 수 없습니다)
            if (timeSinceLastTap < 0.1f)
            {
                return;
            }

            // 더블탭 시간 내에 들어왔는지?
            if (timeSinceLastTap < doubleTapSpeed)
            {
                AttemptDash(); // 대시 발동!
                lastTapTime = 0; // 초기화 (연속 3번 터치했을 때 또 나가는 거 방지)
            }
            else
            {
                lastTapTime = Time.time; // 첫 번째 탭으로 기록
            }
        }
    }

    void FixedUpdate()
    {
        if (IsSpawned && !IsOwner) return;
        if (IsDashing) return;

        if (canMove)
        {
            rb.linearVelocity = movementInput * moveSpeed;
        }
    }

    private void AttemptDash()
    {
        if (!gameObject.activeInHierarchy) return;
        if (!canDash || IsDashing || !canMove) return;

        Vector2 dashDir = movementInput.magnitude > 0 ? movementInput.normalized : lastMoveDirection;
        StartCoroutine(DashRoutine(dashDir));
    }

    IEnumerator DashRoutine(Vector2 dir)
    {
        IsDashing = true;
        canDash = false;

        rb.linearVelocity = dir * dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        rb.linearVelocity = Vector2.zero;
        IsDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
}