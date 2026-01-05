using UnityEngine;

public class SafeAreaHandler : MonoBehaviour
{
    RectTransform rectTransform;
    Rect safeArea;
    Vector2 minAnchor;
    Vector2 maxAnchor;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        Refresh();
    }

    void Update()
    {
        // 화면 회전하거나 해상도 바뀔 때 갱신 (테스트용)
        // 성능이 걱정되면 Update는 지워도 됨
        Refresh();
    }

    void Refresh()
    {
        safeArea = Screen.safeArea;

        // Safe Area를 Unity 앵커 좌표(0~1)로 변환
        minAnchor = safeArea.position;
        maxAnchor = safeArea.position + safeArea.size;

        minAnchor.x /= Screen.width;
        minAnchor.y /= Screen.height;
        maxAnchor.x /= Screen.width;
        maxAnchor.y /= Screen.height;

        // 적용
        rectTransform.anchorMin = minAnchor;
        rectTransform.anchorMax = maxAnchor;
    }
}