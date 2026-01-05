using UnityEngine;
using TMPro;

public class LobbyNickName : MonoBehaviour
{
    [Header("머리 위 텍스트")]
    public TextMeshPro nameText;

    [Header("입력칸 연결")]
    public TMP_InputField inputField;

    void Start()
    {
        // 1. 저장된 이름 있으면 불러오기
        string savedName = PlayerPrefs.GetString("MyNickName", "Player");
        if (inputField != null) inputField.text = savedName;
        UpdateName(savedName);

        // 2. 입력할 때마다 실행될 함수 연결
        if (inputField != null)
        {
            inputField.onValueChanged.AddListener(UpdateName);
        }
    }

    // 글자 칠 때마다 머리 위 텍스트 변경
    public void UpdateName(string newName)
    {
        if (nameText != null)
        {
            nameText.text = string.IsNullOrEmpty(newName) ? "Player" : newName;
        }
    }
}