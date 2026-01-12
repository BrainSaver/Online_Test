using UnityEngine;
using TMPro; // 닉네임 입력창(TextMeshPro) 쓸 거니까 필수

public class UserInfoManager : MonoBehaviour
{
    [Header("연결할 것들")]
    public TMP_InputField nameInput;       // 닉네임 입력칸
    public CharacterColorChanger colorScript; // 색깔 바꾸는 스크립트

    void Start()
    {
        // 게임 켜지자마자 저장된 거 불러오기
        LoadUserData();
    }

    // 저장하기 (게임 시작 버튼 누를 때 호출)
    public void SaveUserData()
    {
        // 1. 닉네임 저장
        PlayerPrefs.SetString("MyNickName", nameInput.text);

        // 2. 색깔 번호 저장
        PlayerPrefs.SetInt("MyColorIndex", colorScript.currentIndex);

        // 3. 디스크에 확실히 기록 (유니티 6에선 자동이지만, 습관적으로 해두면 좋음)
        PlayerPrefs.Save();

        Debug.Log("저장 완료! 이름: " + nameInput.text + ", 색번: " + colorScript.currentIndex);
    }

    // 불러오기 (Start에서 자동 호출)
    void LoadUserData()
    {
        // 1. 닉네임 불러오기 (저장된 게 없으면 "Player"가 기본값)
        if (PlayerPrefs.HasKey("MyNickName"))
        {
            nameInput.text = PlayerPrefs.GetString("MyNickName");
        }
        else
        {
            nameInput.text = "Player";
        }

        // 2. 색깔 불러오기 (저장된 게 없으면 0번)
        if (PlayerPrefs.HasKey("MyColorIndex"))
        {
            int savedIndex = PlayerPrefs.GetInt("MyColorIndex");
            colorScript.SetColorIndex(savedIndex); // 색깔 스크립트에 적용!
        }
    }
}