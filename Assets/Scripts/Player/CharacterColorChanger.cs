using UnityEngine;
using UnityEngine.UI; // 버튼 기능을 쓰기 위해 필수
using System.Collections.Generic;

public class CharacterColorChanger : MonoBehaviour
{
    [Header("1. 바꿀 색상들 (머티리얼)")]
    public List<Material> colorMaterials;
    public SpriteRenderer targetRenderer;

    [Header("2. 버튼을 여기에 드래그해서 넣으세요")]
    public Button prevButton; // 이전 버튼 (<)
    public Button nextButton; // 다음 버튼 (>)

    // 저장용 변수
    public int currentIndex = 0;

    void Start()
    {
        // 1. 다음 버튼 연결
        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners(); // 혹시 모를 중복 제거
            nextButton.onClick.AddListener(NextColor);
        }

        // 2. 이전 버튼 연결 (여기가 안 되셨던 부분)
        if (prevButton != null)
        {
            prevButton.onClick.RemoveAllListeners(); // 혹시 모를 중복 제거
            prevButton.onClick.AddListener(PrevColor);
        }

        // 초기 색상 적용
        UpdateColor();
    }

    // 다음 색상 바꾸기
    public void NextColor()
    {
        currentIndex++;
        if (currentIndex >= colorMaterials.Count) currentIndex = 0;
        UpdateColor();
    }

    // 이전 색상 바꾸기
    public void PrevColor()
    {
        currentIndex--;
        if (currentIndex < 0) currentIndex = colorMaterials.Count - 1; // 0보다 작아지면 맨 뒤로
        UpdateColor();
    }

    // 저장된 데이터 불러오기용
    public void SetColorIndex(int index)
    {
        currentIndex = index;
        if (currentIndex < 0 || currentIndex >= colorMaterials.Count) currentIndex = 0;
        UpdateColor();
    }

    void UpdateColor()
    {
        if (targetRenderer != null && colorMaterials.Count > 0)
        {
            targetRenderer.material = colorMaterials[currentIndex];
        }
    }
}