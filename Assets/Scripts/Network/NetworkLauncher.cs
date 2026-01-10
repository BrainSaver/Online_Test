using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Fusion; // 퓨전 네임스페이스
using UnityEngine.SceneManagement;

public class NetworkLauncher : MonoBehaviour
{
    [Header("NetworkRunner 프리팹을 여기에 넣으십시오")]
    public NetworkRunner runnerPrefab;

    private NetworkRunner _runner;

    // [버튼 1] 방 만들기 (Create Room)
    public void OnClick_CreateRoom()
    {
        // Host 모드: 내가 방장이 되어 세션을 엽니다.
        // 세션 이름을 랜덤으로 설정해 중복을 피합니다.
        StartGame(GameMode.Host, System.Guid.NewGuid().ToString());
    }

    // [버튼 2] 랜덤 입장 (Random Join) -> 빠른 시작
    public void OnClick_JoinRandom()
    {
        // AutoHostOrClient: 방이 있으면 들어가고, 없으면 내가 방을 만듭니다.
        // 구현하기 까다로운 랜덤 매칭을 대체하는 가장 간단한 방법입니다.
        StartGame(GameMode.AutoHostOrClient, "TestSession");
    }

    async void StartGame(GameMode mode, string sessionName)
    {
        // 러너가 없으면 생성
        if (_runner == null)
        {
            _runner = Instantiate(runnerPrefab);
        }

        // 퓨전 2의 핵심: 게임 시작 (비동기 처리)
        await _runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = sessionName,
            // 씬 관리자 자동 추가 (중요)
            SceneManager = _runner.gameObject.AddComponent<NetworkSceneManagerDefault>()
        });

        // 참고: StartGame이 완료되면 Fusion이 자동으로 Build Settings에 있는 다음 씬을 로드합니다.
        // (NetworkRunner에 'Auto Load Scene'이 켜져 있어야 함)
    }
}