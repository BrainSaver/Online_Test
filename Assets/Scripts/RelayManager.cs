using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay;
using Unity.Services.Relay.Models; // 이게 있어야 .ToRelayServerData()가 작동함
using Unity.Networking.Transport.Relay;
using UnityEngine;
using TMPro;

public class RelayManager : MonoBehaviour
{
    [Header("UI 연결")]
    public TMP_Text codeText;
    public TMP_InputField joinInput;
    public GameObject buttonsGroup;

    private async void Start()
    {
        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("로그인 완료: " + AuthenticationService.Instance.PlayerId);
        }
    }

    // [방장] 방 만들기
    public async void CreateRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);
            string joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            if (codeText != null) codeText.text = "Code: " + joinCode;
            Debug.Log($"방 생성 성공! 코드: {joinCode}");

            // [수정된 부분] 생성자(new) 대신 확장 메서드 사용
            RelayServerData relayServerData = allocation.ToRelayServerData("dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartHost();

            if (buttonsGroup != null) buttonsGroup.SetActive(false);
        }
        catch (RelayServiceException e)
        {
            Debug.LogError("방 생성 실패: " + e);
        }
    }

    // [참가자] 방 들어가기
    public async void JoinRelay()
    {
        string joinCode = joinInput.text.ToUpper();

        if (string.IsNullOrEmpty(joinCode))
        {
            Debug.Log("코드를 입력하세요.");
            return;
        }

        try
        {
            Debug.Log($"접속 시도 중... 코드: {joinCode}");

            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);

            // [수정된 부분] 여기도 똑같이 수정
            RelayServerData relayServerData = joinAllocation.ToRelayServerData("dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartClient();

            if (buttonsGroup != null) buttonsGroup.SetActive(false);
        }
        catch (RelayServiceException e)
        {
            Debug.LogError("방 참가 실패: " + e);
        }
    }
}