using System;
using UnityEngine;

[DisallowMultipleComponent]
public class BossHealth : MonoBehaviour
{
    // 전역 접근을 위해(보스는 보통 1체) 인스턴스 보관
    public static BossHealth Instance { get; private set; }

    [Header("HP")]
    public float maxHP = 100f;
    public float hp;

    [Header("Collision Radius (중앙 충돌용)")]
    [Tooltip("보스의 히트 반경(원). 플레이어 탄과 원-원 거리 판정에 사용")]
    public float radius = 0.6f;

    [Header("BossUI")]
    public BossUI BossUI;

    public event Action onDeath;
    public event Action<float, float> onHpChanged; // (hp, max)

    void Awake()
    {
        if (Instance && Instance != this)
            Debug.LogWarning("[BossHealth] 이미 다른 인스턴스가 있습니다.", this);
        Instance = this;
    }

    void OnEnable()
    {
        hp = maxHP;
        if (!BossUI) BossUI = FindAnyObjectByType<BossUI>();
        onHpChanged?.Invoke(hp, maxHP);
        if (BossUI) BossUI.BindBoss(this);
    }

    void OnDisable()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>중앙 충돌 매니저가 호출하는 데미지 입력</summary>
    public void Hit(float damage)
    {
        TakeDamage(damage);
    }

    public void TakeDamage(float damage)
    {
        hp -= damage;
        onHpChanged?.Invoke(hp, maxHP);

        if (hp <= 0f)
        {
            onDeath?.Invoke();
            // 필요 시 보스 종료 처리(예: 연출 후 비활성/파괴)
            gameObject.SetActive(false);
        }
    }
}
