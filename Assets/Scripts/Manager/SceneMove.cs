using UnityEngine;
using UnityEngine.UI;

public class SceneMove : MonoBehaviour
{
    [Header("1. 이 버튼 누르면 켜질 화면들 (여러 개 가능)")]
    public GameObject[] turnOnObjects;

    [Header("2. 이 버튼 누르면 꺼질 화면들 (여러 개 가능)")]
    public GameObject[] turnOffObjects;

    private Button myButton;

    void Start()
    {
        // 버튼을 자동으로 찾아서 클릭 기능 연결
        myButton = GetComponent<Button>();
        if (myButton != null)
        {
            myButton.onClick.AddListener(OnClickButton);
        }
    }

    void OnClickButton()
    {
        // 1. 꺼질 것들 처리 (Active False)
        if (turnOffObjects != null)
        {
            foreach (GameObject obj in turnOffObjects)
            {
                if (obj != null) obj.SetActive(false);
            }
        }

        // 2. 켜질 것들 처리 (Active True)
        if (turnOnObjects != null)
        {
            foreach (GameObject obj in turnOnObjects)
            {
                if (obj != null) obj.SetActive(true);
            }
        }
    }
}