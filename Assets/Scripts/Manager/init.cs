using UnityEngine;

public class init : MonoBehaviour
{
    [Header("1. 관리할 캔버스/부모 (이 밑에 있는 건 일단 다 꺼짐)")]
    public GameObject rootCanvas;

    [Header("2. 그 중에서 켜고 싶은 화면들 (이것만 켜짐)")]
    public GameObject[] turnOnObjects;

    void Start()
    {
        // 1단계: 지정한 부모(Canvas) 아래에 있는 모든 자식들을 싹 다 끕니다.
        if (rootCanvas != null)
        {
            foreach (Transform child in rootCanvas.transform)
            {
                // 자식 오브젝트 하나하나를 비활성화(Active False)
                child.gameObject.SetActive(false);
            }
        }

        // 2단계: 켜라고 목록에 넣은 것들만 다시 켭니다.
        if (turnOnObjects != null)
        {
            foreach (GameObject obj in turnOnObjects)
            {
                if (obj != null) obj.SetActive(true);
            }
        }
    }
}