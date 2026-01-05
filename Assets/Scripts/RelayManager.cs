using System.Collections.Generic;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RelayManager : MonoBehaviour
{
    [Header("UI 연결")]
    public TMP_InputField roomNameInput; // 방 이름 입력칸
    public TMP_InputField nickNameInput; // 닉네임 입력칸
    public Toggle isPublicToggle;        // 공개/비공개 체크
    public Transform roomListContent;    // 방 목록 표시될 곳 (Content)
    public GameObject roomItemPrefab;    // 방 목록 아이템 프리팹
    public GameObject lobbyPanel;        // 로비 전체 UI (LobbyUI)

    [Header("로비 캐릭터")]
    public GameObject lobbyCharacter;    // 화면에 있는 Player

    private Lobby hostLobby;
    private float heartbeatTimer;

    private async void Start()
    {
        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        // 시작 시 닉네임 자동 로드
        if (nickNameInput == null) nickNameInput = FindFirstObjectByType<TMP_InputField>();
        if (nickNameInput != null)
        {
            nickNameInput.text = PlayerPrefs.GetString("MyNickName", "");
        }
    }

    private void Update()
    {
        HandleLobbyHeartbeat();
    }

    // 닉네임 저장
    public void SaveNickName()
    {
        string name = "Player";
        if (nickNameInput != null && !string.IsNullOrEmpty(nickNameInput.text))
        {
            name = nickNameInput.text;
        }
        PlayerPrefs.SetString("MyNickName", name);
        PlayerPrefs.Save();
    }

    // 방 만들기
    public async void CreateLobby()
    {
        SaveNickName();

        try
        {
            string roomName = roomNameInput != null ? roomNameInput.text : "New Room";
            if (string.IsNullOrEmpty(roomName)) roomName = "New Room";
            bool isPrivate = isPublicToggle != null && !isPublicToggle.isOn;

            // 1. Relay 방 생성
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(3);
            string relayCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            // 2. Lobby 생성 (Relay 코드 숨김)
            CreateLobbyOptions options = new CreateLobbyOptions
            {
                IsPrivate = isPrivate,
                Data = new Dictionary<string, DataObject>
                {
                    { "RelayCode", new DataObject(DataObject.VisibilityOptions.Member, relayCode) }
                }
            };

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(roomName, 4, options);
            hostLobby = lobby;

            // 3. 호스트 시작
            var relayServerData = allocation.ToRelayServerData("dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
            NetworkManager.Singleton.StartHost();

            // 로비 캐릭터 숨기기
            if (lobbyCharacter != null) lobbyCharacter.SetActive(false);
            if (lobbyPanel != null) lobbyPanel.SetActive(false);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    // 방 목록 새로고침
    public async void RefreshRoomList()
    {
        try
        {
            if (roomListContent == null) return;

            foreach (Transform child in roomListContent) Destroy(child.gameObject);

            QueryLobbiesOptions options = new QueryLobbiesOptions
            {
                Count = 20,
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT),
                    new QueryFilter(QueryFilter.FieldOptions.IsLocked, "0", QueryFilter.OpOptions.EQ)
                }
            };

            QueryResponse response = await LobbyService.Instance.QueryLobbiesAsync(options);

            foreach (Lobby lobby in response.Results)
            {
                if (roomItemPrefab == null) continue;

                GameObject newItem = Instantiate(roomItemPrefab, roomListContent);
                // RoomItem 스크립트가 있다면 세팅
                var itemScript = newItem.GetComponent<RoomItem>();
                // RoomItem에서 manager 변수가 RelayManager 타입이 아니라 LobbyManager 타입으로 되어있을 수 있음.
                // 일단 SendMessage나 GetComponent로 처리하거나, RoomItem.cs도 RelayManager를 쓰게 고쳐야 함.
                // 편의상 여기서는 생략하고, 아래 RoomItem 수정 코드도 같이 드립니다.
                if (itemScript != null) itemScript.Setup(lobby, this);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    // 랜덤 입장
    public async void JoinRandom()
    {
        SaveNickName();

        try
        {
            Lobby lobby = await LobbyService.Instance.QuickJoinLobbyAsync();
            JoinRelay(lobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log("들어갈 방이 없습니다.");
        }
    }

    // 선택 입장
    public async void JoinLobbyById(string lobbyId)
    {
        SaveNickName();

        try
        {
            Lobby lobby = await LobbyService.Instance.JoinLobbyByIdAsync(lobbyId);
            JoinRelay(lobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.LogError(e);
        }
    }

    private async void JoinRelay(Lobby lobby)
    {
        string relayCode = lobby.Data["RelayCode"].Value;

        var joinAllocation = await RelayService.Instance.JoinAllocationAsync(relayCode);
        var relayServerData = joinAllocation.ToRelayServerData("dtls");

        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);
        NetworkManager.Singleton.StartClient();

        if (lobbyCharacter != null) lobbyCharacter.SetActive(false);
        if (lobbyPanel != null) lobbyPanel.SetActive(false);
    }

    private async void HandleLobbyHeartbeat()
    {
        if (hostLobby != null)
        {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer < 0f)
            {
                float heartbeatTimerMax = 15f;
                heartbeatTimer = heartbeatTimerMax;
                await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
            }
        }
    }
}