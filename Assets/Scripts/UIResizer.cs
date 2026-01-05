using UnityEngine;
using UnityEngine.UI;

public class UIResizer : MonoBehaviour
{
    [Header("화면 가로 대비 비율 (0.1 = 10%, 0.2 = 20%)")]
    [Range(0f, 1f)] public float widthRatio = 0.2f;

    [Header("정사각형 유지 여부")]
    public bool keepAspectRatio = true;

    private RectTransform rectTransform;
    private Canvas parentCanvas;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        // 가장 가까운 부모 캔버스를 찾음
        parentCanvas = GetComponentInParent<Canvas>();

        ApplySize();
    }

    // 화면 회전 등을 대비해 업데이트에서도 체크 (성능 필요시 Start에서만 호출)
    void Update()
    {
        ApplySize();
    }

    void ApplySize()
    {
        if (parentCanvas == null) return;

        // 캔버스의 현재 실제 크기(Reference Resolution 아님, 실제 렌더링 크기)를 가져옴
        RectTransform canvasRect = parentCanvas.GetComponent<RectTransform>();

        // 기준이 될 너비 (가로/세로 중 작은 쪽을 기준으로 할지, 가로를 기준으로 할지 결정)
        // 보통 모바일 게임은 가로가 기니까 'Height'를 기준으로 잡는 게 더 안전할 수도 있지만,
        // 여기서는 직관적으로 Canvas의 가로(Width)를 기준으로 계산합니다.
        float canvasWidth = canvasRect.rect.width;

        // 목표 크기 계산
        float targetSize = canvasWidth * widthRatio;

        if (keepAspectRatio)
        {
            // 가로 세로 똑같이 (정사각형)
            rectTransform.sizeDelta = new Vector2(targetSize, targetSize);
        }
        else
        {
            // 비율만 맞춤 (원래 비율 유지하고 싶으면 별도 로직 필요)
            rectTransform.sizeDelta = new Vector2(targetSize, targetSize);
        }

        // 만약 조이스틱 핸들(손잡이)의 이동 범위(Handle Range)도 같이 늘려야 한다면?
        // MobileController가 붙어있는 경우 자동으로 range 업데이트
        MobileController controller = GetComponent<MobileController>();
        if (controller != null)
        {
            // 보통 배경 크기의 40% 정도가 적당함
            controller.handleRange = targetSize * 0.4f;
        }
    }
}