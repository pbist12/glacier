// File: BossPhaseController.cs (v2 - no homing here)
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Game.Data;

[DisallowMultipleComponent]
public class BossPhaseController : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private BossAsset boss;          // 전체 보스 데이터
    [SerializeField] private int startPhaseIndex = 0; // 시작 페이즈 인덱스(0부터)

    [Header("Refs")]
    [SerializeField] private EnemyHealth health;       // HP/사망 이벤트
    [SerializeField] private BulletPoolHub bulletPool; // 탄 일괄 제거
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
    }

    #region Unity Event
    void OnEnable()
    {
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

        if (enableMove)
        {
            float x = Mathf.PingPong(Time.time * moveSpeed, moveRange * 2f) - moveRange;
            transform.position = new Vector3(x, transform.position.y, transform.position.z);
        }

        _elapsed += Time.deltaTime;

        // 전환 검사
        var next = EvaluateTransition();
        if (next == TransitionOutcome.Next)
        {
            int i = _phaseIndex + 1;
            if (i < boss.phases.Count) ApplyPhase(i);
            else EndEncounter(); // 마지막 다음 → 종료
        }
        else if (next == TransitionOutcome.End)
        {
            EndEncounter();
        }
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
    private TransitionOutcome EvaluateTransition()
    {
        var entry = boss.phases[_phaseIndex];

        // 최소 체류시간 전에는 전환 무시
        if (_elapsed < entry.minSecondsInPhase) return default;

        // 규칙 리스트(위→아래) 첫 매치
        var rules = entry.transitions;
        for (int r = 0; r < rules.Count; r++)
        {
            var rule = rules[r];
            switch (rule.kind)
            {
                case SimpleRuleKind.HPBelow:
                    if (_hp01 <= rule.value) return entry.onRule;
                    break;
                case SimpleRuleKind.TimerReached:
                    if (_elapsed >= rule.value) return entry.onRule;
                    break;
            }
        }

        // 하드 타임아웃(안전망)
        if (entry.hardTimeoutSeconds > 0f && _elapsed >= entry.hardTimeoutSeconds)
            return entry.onTimeout;

        return default; // 전환 없음
    }
    #endregion

    // ===== 페이즈 적용 =====
    private void ApplyPhase(int newIndex)
    {
        _phaseIndex = newIndex;
        _elapsed = 0f;

        // 안전장치: 기존 탄막 정리
        if (bulletPool) bulletPool.BombClearAll();

        // (임시) 기존 스프레드 슈터 ON — 실제로는 패턴 스케줄러가 담당할 예정
        if (spread) spread.isFire = true;

        // TODO: Run-Once 패턴 스케줄러 연결 지점
        // - boss.phases[_phaseIndex].phase.cadenceSeconds 기준으로
        // - patterns[]에서 PatternSOBase를 뽑아 실행
        // - Telegraph/Action/PostDelay를 반영
    }

    // ===== 종료 =====
    private void EndEncounter()
    {
        if (bulletPool) bulletPool.BombClearAll();
        // TODO: 클리어 연출/보상/포털 등
        Destroy(gameObject);
    }
}
