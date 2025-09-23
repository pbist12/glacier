// File: EnemyHealth.cs (Unified: Enemy + Boss) — Pooling 연동 버전
using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class EnemyHealth : MonoBehaviour
{
    // ─────────────────────────────────────────────────────────────
    // 전역 관리(스폰너가 ActiveCount로 참조)
    public static readonly List<EnemyHealth> All = new();
    public static int ActiveCount => All.Count;

    // 현재 필드의 보스(있다면). Boss/FinalBoss일 때만 설정됨.
    public static EnemyHealth CurrentBoss { get; private set; }

    public enum EnemyKind { Normal, Elite, Boss, FinalBoss }

    // ─────────────────────────────────────────────────────────────
    [Header("Identity")]
    [Tooltip("적의 등급/종류")]
    public EnemyKind kind = EnemyKind.Normal;

    [Tooltip("스폰한 주인(엘리트 전멸 체크 등 그룹 진행도 판단에 사용, 선택)")]
    public EnemySpawner owner;

    // ─────────────────────────────────────────────────────────────
    [Header("HP")]
    [Tooltip("최대 체력")]
    public float maxHP = 3f;

    [Tooltip("현재 체력(런타임)")]
    [SerializeField] private float hp;

    // ─────────────────────────────────────────────────────────────
    [Header("Score / Drop")]
    [Tooltip("처치 시 획득 점수")]
    public int scoreOnKill = 10;

    [Tooltip("드롭 담당(선택)")]
    public EnemyDrop enemyDrop;

    // ─────────────────────────────────────────────────────────────
    [Header("Auto Despawn Bounds (옵션)")]
    [Tooltip("화면 바깥 멀리 벗어나면 보상 없이 제거")]
    public bool useBoundsDespawn = true;

    [Tooltip("|x| > bounds.x 또는 |y| > bounds.y 면 제거")]
    public Vector2 bounds = new Vector2(20f, 12f);

    // ─────────────────────────────────────────────────────────────
    [Header("Collision Radius (중앙 충돌용)")]
    [Tooltip("원-원 거리 판정에 사용하는 히트 반경")]
    public float radius = 0.25f;

    // ─────────────────────────────────────────────────────────────
    [Header("Boss UI (보스 전용)")]
    [Tooltip("보스 HP 표시용 UI. Boss/FinalBoss에서만 사용")]
    public BossUI bossUI;

    // ─────────────────────────────────────────────────────────────
    // 이벤트
    public event Action onDeath;
    /// <summary> (hp, maxHP) </summary>
    public event Action<float, float> onHpChanged;

    // ─────────────────────────────────────────────────────────────
    // 프로퍼티/도우미
    public float HP => hp;
    public float HPPercent => maxHP > 0f ? Mathf.Clamp01(hp / maxHP) : 0f;
    public bool IsBossLike => kind == EnemyKind.Boss || kind == EnemyKind.FinalBoss || kind == EnemyKind.Elite;

    // ====== 라이프사이클 ======
    void OnEnable()
    {
        hp = maxHP;
        owner = GameObject.FindFirstObjectByType<EnemySpawner>();
        if (!All.Contains(this)) All.Add(this);

        // 보스면 전역 포인터/보스UI 바인딩
        if (IsBossLike)
        {
            CurrentBoss = this;
            if (!bossUI) bossUI = FindAnyObjectByType<BossUI>();
            onHpChanged?.Invoke(hp, maxHP);
            if (bossUI) bossUI.BindBossLike(this);
        }
        else
        {
            onHpChanged?.Invoke(hp, maxHP);
        }
    }

    void OnDisable()
    {
        All.Remove(this);

        if (IsBossLike && CurrentBoss == this)
        {
            if (bossUI) bossUI.UnbindBoss();
            CurrentBoss = null;
        }
    }

    void Update()
    {
        if (!useBoundsDespawn) return;

        Vector3 p = transform.position;
        if (Mathf.Abs(p.x) > bounds.x || Mathf.Abs(p.y) > bounds.y)
        {
            DespawnWithoutReward(); // ⚠️ 점수/드롭/진행도 미반영
        }
    }

    // ====== 외부에서 호출 ======
    /// <summary>중앙 충돌 매니저가 호출(원형 판정 등)</summary>
    public void Hit(float damage) => TakeDamage(damage);

    /// <summary>데미지 처리(이벤트/보스UI 반영 포함)</summary>
    public void TakeDamage(float damage)
    {
        if (hp <= 0f) return;

        hp -= damage;
        onHpChanged?.Invoke(hp, maxHP);

        if (hp <= 0f)
        {
            // 0 미만으로 내려갔을 때 UI에 0으로 갱신
            hp = 0f;
            onHpChanged?.Invoke(hp, maxHP);
            Die();
        }
    }

    /// <summary>
    /// 스폰 직후 스포너가 타입/오너/체력/점수 등을 세팅할 때 사용.
    /// </summary>
    public void Init(EnemySpawner owner, EnemyKind kind, float? overrideHP = null, int? overrideScore = null)
    {
        this.owner = owner;
        this.kind = kind;
        if (overrideHP.HasValue) { maxHP = overrideHP.Value; hp = maxHP; }
        if (overrideScore.HasValue) scoreOnKill = overrideScore.Value;
    }

    // ====== 내부 처리 ======
    void Die()
    {
        // 보스 UI 언바인딩(보스인 경우) + 스폰 타이머 리셋을 "소유 스포너"에만 통지
        if (IsBossLike && bossUI)
        {
            bossUI.UnbindBoss();
            owner?.ResetTick();  // FindFirstObjectByType 대신 오너 참조 사용
        }

        // 점수 및 게임 진행 이벤트
        var gm = GameManager.Instance;
        if (gm)
        {
            switch (kind)
            {
                case EnemyKind.Normal:
                    gm.OnEnemyKilled(false, scoreOnKill);
                    break;

                case EnemyKind.Elite:
                    gm.OnEliteUnitKilled(scoreOnKill);
                    if (owner) owner.NotifyEliteUnitDead();
                    break;

                case EnemyKind.Boss:
                    gm.OnBossKilled(scoreOnKill);
                    break;

                case EnemyKind.FinalBoss:
                    gm.AddScore(scoreOnKill);
                    gm.ShowResult();
                    break;
            }
        }

        // 드롭
        if (enemyDrop) enemyDrop.DropItem();

        // 이벤트 통지(보스/일반 공통)
        onDeath?.Invoke();

        // 제거(오브젝트 풀 사용 시 SetActive(false)로 교체)
        var hub = owner ? owner.Hub : null;
        if (owner && owner.usePooling && hub != null)
            hub.Despawn(gameObject);
        else
            Destroy(gameObject);
    }

    // 화면 밖 이탈 등 “보상 없는 제거”
    void DespawnWithoutReward()
    {
        // 보스 UI는 언바인드(보스일 때만) — 보상 없는 제거에서도 UI가 남지 않도록 처리
        if (IsBossLike && bossUI) bossUI.UnbindBoss();

        // 풀 반납(없으면 Destroy)
        var hub = owner ? owner.Hub : null;
        if (owner && owner.usePooling && hub != null)
            hub.Despawn(gameObject);
        else
            Destroy(gameObject);
    }
}
