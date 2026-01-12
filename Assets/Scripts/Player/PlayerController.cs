using UnityEngine;

// 이 스크립트는 Rigidbody2D 컴포넌트가 없으면 자동으로 추가해줍니다 (실수 방지)
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("이동 설정")]
    [Tooltip("캐릭터의 이동 속도입니다.")]
    public float moveSpeed = 5f;

    [Header("조이스틱 연결")]
    [Tooltip("Variable Joystick을 여기에 연결하세요. 비워두면 자동으로 찾습니다.")]
    public Joystick joystick;

    // 내부 컴포넌트 캐싱
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Vector2 moveInput;

    void Awake()
    {
        // 컴포넌트 가져오기
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();

        // 조이스틱이 연결되지 않았다면 씬에서 자동으로 찾음
        // Unity 6 최적화: FindObjectOfType 대신 FindFirstObjectByType 사용 (경고 제거)
        if (joystick == null)
        {
            joystick = Object.FindFirstObjectByType<Joystick>();
        }
    }

    void Update()
    {
        // 조이스틱이 없으면 오류 방지를 위해 리턴
        if (joystick == null) return;

        // 1. 입력 감지
        moveInput.x = joystick.Horizontal;
        moveInput.y = joystick.Vertical;

        // 2. 캐릭터 방향 전환 (Sprite Flip)
        // 입력이 있을 때만 방향을 바꿈
        if (moveInput.x != 0 && sr != null)
        {
            sr.flipX = moveInput.x < 0;
        }
    }

    void FixedUpdate()
    {
        // 3. 물리 이동 처리 (Unity 6 변경 사항 적용)
        // 경고 원인 제거: rb.velocity 대신 rb.linearVelocity 사용
        rb.linearVelocity = moveInput * moveSpeed;
    }
}