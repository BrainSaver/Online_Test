using Unity.Netcode;
using UnityEngine;
using TMPro;
using Unity.Collections;

public class PlayerName : NetworkBehaviour
{
    [SerializeField] private TextMeshPro nameText;
    public NetworkVariable<FixedString32Bytes> netName = new NetworkVariable<FixedString32Bytes>();

    public override void OnNetworkSpawn()
    {
        // 1. 값이 바뀌면 텍스트 갱신 (다른 사람용)
        netName.OnValueChanged += (oldValue, newValue) => {
            nameText.text = newValue.ToString();
        };

        if (IsOwner)
        {
            // 2. [수정] 내 거는 PlayerPrefs에서 가져와서 '즉시' 적용
            string myName = PlayerPrefs.GetString("MyNickName", "Player");
            nameText.text = myName; // ★ 기다리지 말고 바로 띄우기

            // 3. 그리고 서버에 알려주기
            SetNameServerRpc(myName);
        }
        else
        {
            // 남의 거는 네트워크 값으로 표시
            nameText.text = netName.Value.ToString();
        }
    }

    [ServerRpc]
    private void SetNameServerRpc(string name)
    {
        netName.Value = name;
    }
}