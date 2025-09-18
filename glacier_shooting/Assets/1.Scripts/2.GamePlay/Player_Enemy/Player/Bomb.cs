using UnityEngine;
using UnityEngine.Events;

public class Bomb : MonoBehaviour
{
    [Header("Refs")]
    public BulletPoolHub hub;                 // 비워두면 자동 탐색
    public PlayerInventory playerInventory;

    [Header("Bomb Options")]
    public bool includePlayerBullets = false; // true면 플레이어 탄도 삭제
    public bool useRadius = false;            // true면 반경 삭제 사용
    public float radius = 6f;                 // 반경 크기
    public Transform center;                  // 기준점(비우면 본인 위치)

    [Header("Limits")]
    public float cooldown = 1.5f;             // 폭탄 재사용 대기(초)

    [Header("Events")]
    public UnityEvent onBombFired;            // 연출 훅(플래시/사운드 등)

    float _nextUseTime = 0f;

    void Awake()
    {
        if (!hub) hub = FindFirstObjectByType<BulletPoolHub>();
        if (!playerInventory) playerInventory = FindFirstObjectByType<PlayerInventory>();
        if (!center) center = transform;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
            TryUseBomb();
    }

    public bool TryUseBomb()
    {
        if (!hub) { Debug.LogWarning("[BombTrigger] hub가 없습니다."); return false; }
        if (Time.time < _nextUseTime) return false;
        if (playerInventory.bomb <= 0) return false;

        if (useRadius)
        {
            Vector2 c = center ? (Vector2)center.position : Vector2.zero;
            // 플레이어 탄 제외 예시: b.poolKey != BulletPoolKey.Player
            hub.BombClearInRadius(c, radius, includePlayerBullets ? null : (b => b.poolKey != BulletPoolKey.Player));
        }
        else
        {
            hub.BombClearAll(includePlayerBullets);
        }

        playerInventory.bomb--;
        _nextUseTime = Time.time + cooldown;
        onBombFired?.Invoke();
        return true;
    }
}
