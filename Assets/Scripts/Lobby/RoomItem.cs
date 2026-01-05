using UnityEngine;
using TMPro;
using Unity.Services.Lobbies.Models;

public class RoomItem : MonoBehaviour
{
    public TextMeshProUGUI roomNameText;
    public TextMeshProUGUI playerCountText;

    private Lobby lobby;
    private RelayManager manager; // ★ 여기를 RelayManager로 변경

    public void Setup(Lobby _lobby, RelayManager _manager) // ★ 여기도 변경
    {
        lobby = _lobby;
        manager = _manager;

        roomNameText.text = lobby.Name;
        playerCountText.text = $"{lobby.Players.Count}/{lobby.MaxPlayers}";
    }

    public void OnClickJoin()
    {
        if (manager != null) manager.JoinLobbyById(lobby.Id);
    }
}