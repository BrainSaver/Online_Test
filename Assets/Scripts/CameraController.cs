using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("추적 설정")]
    public Transform target;        // 따라갈 대상
    public float smoothTime = 0.2f; // 따라가는 속도 (낮을수록 빠릿함)
    public float zOffset = -10f;    // 카메라의 Z 위치 고정값

    private Vector3 velocity = Vector3.zero;

    // 플레이어가 소환될 때 호출해서 "나를 봐!"라고 명령하는 함수
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;

        // 타겟을 잡는 순간, 텔레포트해서 바로 비춰줌 (처음에 확 튀는 현상 방지)
        if (target != null)
        {
            transform.position = new Vector3(target.position.x, target.position.y, zOffset);
        }
    }

    void LateUpdate()
    {
        // 타겟이 없으면(아직 접속 안 했거나 죽었으면) 아무것도 안 함
        if (target == null) return;

        // 목표 위치 (타겟의 X, Y + 고정된 Z값)
        Vector3 targetPosition = new Vector3(target.position.x, target.position.y, zOffset);

        // 부드럽게 이동 (SmoothDamp)
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
}