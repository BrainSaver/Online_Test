using UnityEngine;
using UnityEngine.UI; // 버튼 사용

public class CharacterColorChanger : MonoBehaviour
{
    [Header("1. 바꿀 색상들 (머티리얼) - 계속 추가 가능")]
    public Material[] colorMaterials;

    [Header("2. 색이 변할 렌더러 (캐릭터 모델)")]
    public Renderer targetRenderer;

    [Header("3. 버튼 연결")]
    public Button prevButton; // 이전 색 버튼
    public Button nextButton; // 다음 색 버튼

    // 현재 몇 번째 색인지 기억하는 변수
    private int currentIndex = 0;

    void Start()
    {
        // 만약 렌더러를 깜빡하고 안 넣었으면, 내 몸통(GetComponent)에서 찾습니다.
        if (targetRenderer == null)
        {
            targetRenderer = GetComponent<Renderer>();
        }

        // 버튼 기능 연결
        if (nextButton != null) nextButton.onClick.AddListener(OnNextClick);
        if (prevButton != null) prevButton.onClick.AddListener(OnPrevClick);

        // 시작할 때 첫 번째 색으로 세팅
        ApplyColor();
    }

    // 다음 버튼 눌렀을 때
    void OnNextClick()
    {
        if (colorMaterials.Length == 0) return;

        currentIndex++;
        // 마지막 색 다음엔 다시 처음(0번)으로 돌아감
        if (currentIndex >= colorMaterials.Length)
        {
            currentIndex = 0;
        }

        ApplyColor();
    }

    // 이전 버튼 눌렀을 때
    void OnPrevClick()
    {
        if (colorMaterials.Length == 0) return;

        currentIndex--;
        // 처음(0번) 이전엔 마지막 번호로 돌아감
        if (currentIndex < 0)
        {
            currentIndex = colorMaterials.Length - 1;
        }

        ApplyColor();
    }

    // 실제 색을 입히는 함수
    void ApplyColor()
    {
        if (targetRenderer != null && colorMaterials.Length > 0)
        {
            targetRenderer.material = colorMaterials[currentIndex];
        }
    }
}