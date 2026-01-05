using UnityEngine;
using UnityEngine.EventSystems; // 터치 인식용

public class MobileController : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
{
    // 어디서든 접근할 수 있게 싱글톤 처리
    public static MobileController Instance;

    [Header("조이스틱 연결")]
    public RectTransform joystickBG;
    public RectTransform joystickHandle;
    [Range(0, 500)] public float handleRange = 100f; // 핸들 이동 반경

    [Header("입력 값 (PlayerController가 가져갈 값)")]
    public Vector2 moveDirection = Vector2.zero; // 이동 방향
    public bool isDashDown = false; // 대시 버튼 눌림 여부

    private Vector2 joystickOriginalPos;

    void Awake()
    {
        Instance = this; // 나 자신을 전역 변수에 등록
    }

    void Start()
    {
        // 조이스틱의 원래 위치 기억
        if (joystickBG != null) joystickOriginalPos = joystickBG.anchoredPosition;
    }

    // --- 조이스틱 로직 ---
    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData); // 터치하자마자 바로 움직이게
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 pos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            joystickBG,
            eventData.position,
            eventData.pressEventCamera,
            out pos))
        {
            pos.x = (pos.x / joystickBG.sizeDelta.x);
            pos.y = (pos.y / joystickBG.sizeDelta.y);

            // [수정] pos.y * 2  --->  pos.y * -2 (마이너스 추가!)
            // 혹은 반대로, 기존에 마이너스가 있었다면 지워주세요.
            Vector2 inputVector = new Vector2(pos.x * 2, pos.y * 2);

            // 만약 위 코드로 해결이 안 된다면 아래처럼 직접 -를 붙여보세요
            // Vector2 inputVector = new Vector2(pos.x * 2, -(pos.y * 2)); 

            inputVector = (inputVector.magnitude > 1.0f) ? inputVector.normalized : inputVector;

            moveDirection = inputVector;

            joystickHandle.anchoredPosition = new Vector2(
                inputVector.x * handleRange,
                inputVector.y * handleRange
            );
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // 손 떼면 원위치
        moveDirection = Vector2.zero;
        joystickHandle.anchoredPosition = Vector2.zero;
    }

    // --- 대시 버튼 로직 (EventTrigger로 연결할 함수) ---
    public void OnDashBtnDown()
    {
        isDashDown = true;
    }

    public void OnDashBtnUp()
    {
        isDashDown = false; // PlayerController에서 처리 후 꺼줘도 되지만 안전장치
    }
}