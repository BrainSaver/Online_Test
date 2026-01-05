using System.Collections;
using UnityEngine;
using Unity.Netcode; // [필수] 이거 없으면 Teleport 및 IsOwner 못 씀

// [변경 1] IsOwner를 쓰기 위해 MonoBehaviour를 NetworkBehaviour로 변경해야 합니다.
public class PlayerFallHandler : NetworkBehaviour
{
    [Header("낙사 설정")]
    public int fallDamage = 1;
    public float respawnDelay = 1.0f;
    public float invincibleTime = 3.0f;
    public float feetOffset = 0.0f;

    private Vector3 lastSafePosition;
    private bool isFalling = false;
    private bool isInvincible = false;

    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;
    private PlayerController controller;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        controller = GetComponent<PlayerController>();
        lastSafePosition = transform.position;
    }

    void Update()
    {
        // 내 캐릭터가 아니면 안전 위치 갱신도 하지 않음 (불필요한 연산 방지)
        if (IsSpawned && !IsOwner) return;

        // 떨어지는 중이 아니고 무적도 아니면 안전 위치 갱신
        if (!isFalling && !isInvincible)
        {
            lastSafePosition = transform.position;
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        // [중요] 내 캐릭터가 아니면 낙사 로직 자체를 실행하지 않음
        if (IsSpawned && !IsOwner) return;

        if (isFalling || isInvincible) return;
        if (controller != null && controller.IsDashing) return;

        if (collision.CompareTag("FallZone"))
        {
            Vector2 checkPos = new Vector2(transform.position.x, transform.position.y - feetOffset);
            if (collision.OverlapPoint(checkPos))
            {
                StartCoroutine(FallSequence());
            }
        }
    }

    IEnumerator FallSequence()
    {
        // [변경 2] 핵심 수정 사항!
        // 주인(Owner)이 아니면 이 코루틴을 강제 종료합니다.
        // 서버나 다른 클라이언트가 내 위치를 강제로 옮기려 할 때 발생하는 에러를 막습니다.
        if (!IsOwner) yield break;

        isFalling = true;
        if (controller != null) controller.canMove = false;

        // 1. 관성 제거 (Unity 6 호환)
        if (rb != null) rb.linearVelocity = Vector2.zero;

        // 작아지는 연출
        float timer = 0f;
        Vector3 originalScale = transform.localScale;

        // * NetworkTransform이 Scale을 동기화하고 있다면, 
        // 주인이 여기서 줄어들 때 다른 사람들 눈에도 같이 줄어들어 보입니다.
        while (timer < respawnDelay)
        {
            timer += Time.deltaTime;
            float progress = timer / respawnDelay;
            transform.localScale = Vector3.Lerp(originalScale, Vector3.zero, progress);
            yield return null;
        }

        Debug.Log($"낙사! 체력 -{fallDamage}");

        // 2. 위치 원상복구 (네트워크 텔레포트 사용)
        if (TryGetComponent(out Unity.Netcode.Components.NetworkTransform netTransform))
        {
            // 속도 확실히 0으로 죽이고 이동
            if (rb != null) rb.linearVelocity = Vector2.zero;

            // [안전함] 위에서 IsOwner 체크를 했으므로 에러가 나지 않습니다.
            // 텔레포트 하면서 크기도 원래대로(originalScale) 복구합니다.
            netTransform.Teleport(lastSafePosition, transform.rotation, originalScale);
        }
        else
        {
            // 싱글 테스트용
            transform.position = lastSafePosition;
            transform.localScale = originalScale;
        }

        // 혹시 모르니 로컬 스케일 확실하게 복구
        transform.localScale = originalScale;

        StartCoroutine(InvincibilityRoutine());

        isFalling = false;
        if (controller != null) controller.canMove = true;
    }

    IEnumerator InvincibilityRoutine()
    {
        isInvincible = true;
        for (int i = 0; i < 5; i++)
        {
            spriteRenderer.color = new Color(1, 1, 1, 0.5f);
            yield return new WaitForSeconds(invincibleTime / 10f);
            spriteRenderer.color = Color.white;
            yield return new WaitForSeconds(invincibleTime / 10f);
        }
        isInvincible = false;
    }
}