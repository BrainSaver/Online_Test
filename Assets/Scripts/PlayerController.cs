using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems; // UI 감지용
using TMPro; // 입력칸 감지용

public class PlayerController : NetworkBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private VariableJoystick joystick;

    [Header("이동 설정")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float dashSpeed = 15f;
    [SerializeField] private float dashDuration = 0.2f;
    [SerializeField] private float dashCooldown = 1.0f;
    [SerializeField] private float doubleTapSpeed = 0.3f;

    private Rigidbody2D rb;
    private Vector2 movementInput;
    private Vector2 lastMoveDirection = Vector2.right;
    private float lastTapTime = 0f;

    public bool IsDashing { get; private set; }
    public bool canMove = true;
    private bool canDash = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // 프레임 설정
        QualitySettings.vSyncCount = 0;
        int refreshRate = (int)Screen.currentResolution.refreshRateRatio.value;
        if (refreshRate < 60) refreshRate = 60;
        Application.targetFrameRate = refreshRate;
    }

    // ★ 로비(오프라인)에서도 움직일 수 있게 Start에서 연결
    void Start()
    {
        if (!IsSpawned || IsOwner) ConnectControls();
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner) ConnectControls();
    }

    void ConnectControls()
    {
        var cam = Object.FindFirstObjectByType<CameraController>();
        if (cam != null) cam.SetTarget(transform);
        if (joystick == null) joystick = FindFirstObjectByType<VariableJoystick>();
    }

    void Update()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, 0f);

        // 남의 캐릭터는 움직이지 않음
        if (IsSpawned && !IsOwner) return;

        // ★ [핵심] 닉네임 입력 중이면 움직임 멈춤
        if (IsTyping())
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (IsDashing) return;

        // --- 이동 입력 로직 ---
        float inputX = 0;
        float inputY = 0;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed) inputY = 1;
            if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed) inputY = -1;
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) inputX = -1;
            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) inputX = 1;
        }

        if (inputX == 0 && inputY == 0 && joystick != null)
        {
            inputX = joystick.Horizontal;
            inputY = joystick.Vertical;
        }

        if (inputX != 0 || inputY != 0)
        {
            lastMoveDirection = new Vector2(inputX, inputY).normalized;
        }

        CheckDoubleTap();

        if (!IsDashing && canMove)
        {
            Vector2 rawInput = new Vector2(inputX, inputY);
            movementInput = Vector2.ClampMagnitude(rawInput, 1f);
        }
    }

    // ★ 타자 치는 중인지 확인하는 함수
    bool IsTyping()
    {
        if (EventSystem.current == null) return false;
        GameObject selected = EventSystem.current.currentSelectedGameObject;
        if (selected == null) return false;
        // 선택된 오브젝트가 InputField면 true 리턴
        return selected.GetComponent<TMP_InputField>() != null;
    }

    // 더블탭 (사용자님 코드 유지)
    private void CheckDoubleTap()
    {
        bool tappedRightSide = false;
        if (UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches.Count > 0)
        {
            foreach (var touch in UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches)
            {
                if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
                {
                    if (touch.screenPosition.x > Screen.width * 0.5f) tappedRightSide = true;
                }
            }
        }
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Vector2 mousePos = Mouse.current.position.ReadValue();
            if (mousePos.x > Screen.width * 0.5f) tappedRightSide = true;
        }

        if (tappedRightSide)
        {
            float timeSinceLastTap = Time.time - lastTapTime;
            if (timeSinceLastTap < 0.1f) return;

            if (timeSinceLastTap < doubleTapSpeed)
            {
                AttemptDash();
                lastTapTime = 0;
            }
            else lastTapTime = Time.time;
        }
    }

    void FixedUpdate()
    {
        if (IsSpawned && !IsOwner) return;
        if (IsDashing) return;
        if (canMove) rb.linearVelocity = movementInput * moveSpeed;
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

    private void OnEnable() { UnityEngine.InputSystem.EnhancedTouch.EnhancedTouchSupport.Enable(); }
    private void OnDisable() { UnityEngine.InputSystem.EnhancedTouch.EnhancedTouchSupport.Disable(); }
}