using Unity.Netcode.Components; // 이게 없으면 에러남
using UnityEngine;

// 기본 NetworkTransform의 권한을 '클라이언트'에게 주는 스크립트
[DisallowMultipleComponent]
public class ClientNetworkTransform : NetworkTransform
{
    // "서버가 주인인가요?" -> "아니요(false)" 라고 대답함
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}