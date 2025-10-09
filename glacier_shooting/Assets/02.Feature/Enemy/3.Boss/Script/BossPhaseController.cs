// File: BossPhaseController.cs (v2 - no homing here)
using Boss;
using Game.Data;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

[DisallowMultipleComponent]
public class BossPhaseController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private BossAsset boss;          // 전체 보스 데이터
    [SerializeField] private int startPhaseIndex = 0; // 시작 페이즈 인덱스(0부터)

    [Header("Refs")]
    [SerializeField] private EnemyHealth health;       // HP/사망 이벤트
    [SerializeField] private BulletPoolHub bulletPool; // 탄 일괄 제거
    [SerializeField] private BossPatternScheduler patternScheduler;
    [SerializeField] private BossPatternShooter spread; // 기존 부채꼴(임시 데모용)

    [Header("Move (optional demo)")]
    public bool enableMove = false;
    public float moveSpeed = 2f;
    public float moveRange = 2.5f;

    // state
    private int _phaseIndex = -1;
    private float _elapsed; // 현 페이즈 경과 시간
    private float _hp01 = 1f;

    void Reset()
    {
        if (!health) health = GetComponent<EnemyHealth>();
        if (!spread) spread = GetComponent<BossPatternShooter>();
        if (!bulletPool) bulletPool = FindFirstObjectByType<BulletPoolHub>();
        if (!patternScheduler) patternScheduler = GetComponent<BossPatternScheduler>();
    }

    #region Unity Event

    void Awake()
    {
        if (boss && health)
        {
            health.maxHP = boss.maxHealth;
            health.HP = boss.maxHealth;
        }
    }
    void OnEnable()
    {
        Reset();
        if (!boss || boss.phases == null || boss.phases.Count == 0)
        {
            Debug.LogError("[BossPhaseController] BossAsset이 비었거나 페이즈가 없습니다.", this);
            enabled = false;
            return;
        }
        if (health)
        {
            health.onDeath += OnDeath;
            health.onHpChanged += OnHpChanged;
        }
        if (boss && health)
        {
            health.maxHP = boss.maxHealth;
            health.HP = boss.maxHealth;
        }

        ApplyPhase(Mathf.Clamp(startPhaseIndex, 0, boss.phases.Count - 1));
    }
    void OnDisable()
    {
        if (health)
        {
            health.onDeath -= OnDeath;
            health.onHpChanged -= OnHpChanged;
        }
    }

    void Update()
    {
        if (_phaseIndex < 0) return;

        _elapsed += Time.deltaTime;

        // (선택) 진입 직후 쿨다운/스로틀이 있으면 여기 가드 추가

        Debug.Log(health.HP / health.maxHP);
        if (!TryEvaluateTransition(out var outcome))
            return; // ★ 조건이 안 맞음 → 그냥 유지

        if (outcome == TransitionOutcome.Next)
        {
            int i = _phaseIndex + 1;
            if (i < boss.phases.Count) ApplyPhase(i);
            else EndEncounter(); // 마지막 다음이면 종료 (또는 무시 옵션)
        }
        else if (outcome == TransitionOutcome.End)
        {
            EndEncounter();
        }
        // outcome이 Next/End 외 값이 없다면 else는 필요 X
    }
    #endregion

    #region HP Evenet
    // ===== HP 이벤트 =====
    private void OnHpChanged(float hp, float max)
    {
        _hp01 = max > 0f ? Mathf.Clamp01(hp / max) : 0f;
    }

    private void OnDeath()
    {
        EndEncounter();
    }
    #endregion

    #region 전환
    // ===== 전환 로직 =====
    private bool TryEvaluateTransition(out TransitionOutcome outcome)
    {
        outcome = default;
        var entry = boss.phases[_phaseIndex];
        bool hit = (entry.transitions.kind == SimpleRuleKind.HPBelow && (health.HP / health.maxHP) <= entry.transitions.value);

        if (hit)
        {
            outcome = entry.onRule; // Next or End (SO는 그대로)
            return true;            // ★ 매치!
        }

        return false; // 아무 것도 아님 → 유지
    }
    #endregion

    // ===== 페이즈 적용 =====
    private void ApplyPhase(int newIndex)
    {
        _phaseIndex = newIndex;
        _elapsed = 0f;

        // 안전장치: 기존 탄막 정리
        if (bulletPool) bulletPool.BombClearAll();

        if (patternScheduler) patternScheduler.StartPhase(boss.phases[_phaseIndex].phase);

        Debug.Log(boss.phases[_phaseIndex].phase);

        // (임시) 기존 스프레드 슈터 ON — 실제로는 패턴 스케줄러가 담당할 예정
        // if (spread) spread.isFire = true;

        // TODO: Run-Once 패턴 스케줄러 연결 지점
        // - boss.phases[_phaseIndex].phase.cadenceSeconds 기준으로
        // - patterns[]에서 PatternSOBase를 뽑아 실행
        // - Telegraph/Action/PostDelay를 반영
    }

    // ===== 종료 =====
    private void EndEncounter()
    {
        if (bulletPool) bulletPool.BombClearAll();
    }
}
