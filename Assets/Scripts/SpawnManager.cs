using UnityEngine;
using Unity.Netcode;

public class SpawnManager : NetworkBehaviour
{
    [Header("스폰 위치 목록")]
    public Transform[] spawnPoints;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // 방장(호스트) 자신 처리
            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                // 방장은 권한이 서버(자기 자신)에게 있으므로 직접 이동 가능하지만,
                // 통일성을 위해 RPC로 처리하거나 직접 이동시킵니다.
                // 여기서는 방장은 서버이자 주인이므로 직접 이동이 가능하긴 합니다.
                // 하지만 깔끔하게 로직을 통일합시다.
                MovePlayer(client.ClientId);
            }

            // 앞으로 들어올 게스트 처리
            NetworkManager.Singleton.OnClientConnectedCallback += MovePlayer;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= MovePlayer;
        }
    }

    // [서버가 실행] "누가 들어왔네? 쟤 자리 정해서 명령 보내자"
    private void MovePlayer(ulong clientId)
    {
        int index = (int)(clientId % (ulong)spawnPoints.Length);
        Vector3 spawnPos = spawnPoints[index].position;

        // 특정 클라이언트에게만 명령을 보내기 위한 설정
        ClientRpcParams rpcParams = new ClientRpcParams
        {
            Send = new ClientRpcSendParams
            {
                TargetClientIds = new ulong[] { clientId }
            }
        };

        // "야 (clientId)번아, 너 spawnPos 위치로 가라!" 라고 명령
        TeleportClientRpc(spawnPos, rpcParams);
    }

    // [클라이언트가 실행] 명령을 받으면 자기가 직접 이동함
    [ClientRpc]
    private void TeleportClientRpc(Vector3 pos, ClientRpcParams clientRpcParams = default)
    {
        // 내 캐릭터 찾기
        var localPlayer = NetworkManager.Singleton.LocalClient.PlayerObject;

        if (localPlayer != null && localPlayer.TryGetComponent(out Unity.Netcode.Components.NetworkTransform netTransform))
        {
            // 내가 주인이니까 Teleport 가능!
            netTransform.Teleport(pos, Quaternion.identity, Vector3.one);
            Debug.Log($"스폰 위치 {pos}로 이동 완료!");
        }
    }
}