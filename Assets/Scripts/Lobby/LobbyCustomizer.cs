using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class LobbyCustomizer : MonoBehaviour
{
    [Header("연결: 캐릭터")]
    public SpriteRenderer characterRenderer;

    [Header("연결: UI 요소")]
    public TMP_InputField nickNameInput;
    public Button leftBtn;
    public Button rightBtn;
    public Button startBtn;

    [Header("설정: 색깔 목록")]
    public List<Material> colorMaterials;

    [Header("★ 화면 전환 연결")]
    public GameObject customizationPanel; // 지금 보고 있는거 (화살표 등)
    public GameObject lobbyMainPanel;     // 다음에 띄울거 (방 만들기 버튼 등)

    private int currentIndex = 0;

    void Start()
    {
        // 1. 저장된 정보 불러오기
        currentIndex = PlayerPrefs.GetInt("MyColorIndex", 0);
        string savedName = PlayerPrefs.GetString("MyNickName", "");
        if (nickNameInput != null) nickNameInput.text = savedName;

        // 2. 시작할 때 로비 UI는 꺼두고, 커스텀 UI는 켜두기
        if (lobbyMainPanel != null) lobbyMainPanel.SetActive(false);
        if (customizationPanel != null) customizationPanel.SetActive(true);

        UpdateColor();

        leftBtn.onClick.AddListener(PrevColor);
        rightBtn.onClick.AddListener(NextColor);
        startBtn.onClick.AddListener(OnStartGame);
    }

    public void PrevColor()
    {
        currentIndex--;
        if (currentIndex < 0) currentIndex = colorMaterials.Count - 1;
        UpdateColor();
    }

    public void NextColor()
    {
        currentIndex++;
        if (currentIndex >= colorMaterials.Count) currentIndex = 0;
        UpdateColor();
    }

    void UpdateColor()
    {
        if (colorMaterials.Count == 0 || characterRenderer == null) return;
        characterRenderer.material = colorMaterials[currentIndex];
        PlayerPrefs.SetInt("MyColorIndex", currentIndex);
    }

    public void OnStartGame()
    {
        // 1. 닉네임 저장
        string name = "Player";
        if (nickNameInput != null && !string.IsNullOrEmpty(nickNameInput.text))
            name = nickNameInput.text;
        PlayerPrefs.SetString("MyNickName", name);
        PlayerPrefs.Save();

        Debug.Log("설정 완료! 로비로 이동합니다.");

        // 2. ★ UI 교체 (커스텀 끄고 -> 로비 켜기)
        if (customizationPanel != null) customizationPanel.SetActive(false);
        if (lobbyMainPanel != null) lobbyMainPanel.SetActive(true);
    }
}