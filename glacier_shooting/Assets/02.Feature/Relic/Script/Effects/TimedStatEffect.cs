using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 정해진 타이밍(장착/스킬 사용 등)에 스탯(고정/퍼센트)을 적용하고,
/// duration>0이면 n초 뒤 자동으로 원복하는 범용 이펙트.
/// </summary>
[Serializable]
public class TimedStatEffect : ItemEffect
{
    // 1) 무엇(스탯 변화)
    public StatType stat;                    // 예: BulletDamage, FireRate, BulletSpeed, MaxHP, MaxMana ...
    public int flat = 0;                     // +고정치
    [Range(-5f, 5f)] public float percent;   // +퍼센트 (0.25 = +25%)

    // 2) 얼마나(지속/중첩)
    [Tooltip("0이면 영구, >0이면 n초 후 자동 원복")]
    public float duration = 0f;

    public enum StackPolicy { Stack, Refresh, Ignore }
    public StackPolicy stacking = StackPolicy.Refresh;

    // 3) 언제(트리거)
    [Header("Triggers")]
    public bool onEquip = true;        // 유물 장착 시 즉시 적용
    public bool onSkillUsed = false;   // 스킬 사용 직후 적용

    // 내부 상태
    private ItemContext _ctx;
    private BuffHost _host;            // 코루틴 실행용 보조 컴포넌트
    private int _stacks = 0;
    private Coroutine _timer;          // Refresh 정책용 타이머 1개

    public override void Apply(ItemContext ctx)
    {
        _ctx = ctx;

        // ❗ GameObject를 MonoBehaviour로 캐스팅하면 안 됩니다.
        // 코루틴을 돌릴 수 있는 '호스트 컴포넌트'를 GameObject에 붙여 사용합니다.
        if (ctx != null && ctx.owner != null)
        {
            _host = ctx.owner.GetComponent<BuffHost>();
            if (_host == null) _host = ctx.owner.AddComponent<BuffHost>();
        }

        if (onEquip) ApplyOnce();

        if (onSkillUsed)
        {
            GameEvents.SkillUsed += OnSkillUsed;
        }
    }

    public override void Remove(ItemContext ctx)
    {
        if (onSkillUsed)
        {
            GameEvents.SkillUsed -= OnSkillUsed;
        }

        // 진행 중 타이머 정리
        if (_timer != null && _host != null)
        {
            _host.StopCoroutine(_timer);
            _timer = null;
        }

        // 남은 스택 일괄 되돌림
        if (_stacks > 0 && _ctx != null && _ctx.stats != null)
        {
            _ctx.stats.AddModifier(stat, -flat * _stacks, -percent * _stacks);
            _stacks = 0;
        }
    }

    private void OnSkillUsed()
    {
        ApplyOnce();
    }

    private void ApplyOnce()
    {
        if (_ctx == null || _ctx.stats == null) return;

        _ctx.stats.AddModifier(stat, flat, percent);
        _stacks++;

        if (duration > 0f && _host != null)
        {
            switch (stacking)
            {
                case StackPolicy.Refresh:
                    if (_timer != null) _host.StopCoroutine(_timer);
                    _timer = _host.StartCoroutine(RevertAllAfter(duration));
                    break;

                case StackPolicy.Stack:
                    _host.StartCoroutine(RevertOneAfter(duration));
                    break;

                case StackPolicy.Ignore:
                    // 이미 켜져 있으면 추가 적용하지 않음(여기서는 단순 처리: 첫 적용만 유효)
                    if (_stacks > 1)
                    {
                        // 방금 더해진 1스택을 즉시 되돌려서 무시 효과를 만듦
                        _ctx.stats.AddModifier(stat, -flat, -percent);
                        _stacks = _stacks - 1;
                    }
                    break;
            }
        }
    }

    private IEnumerator RevertAllAfter(float t)
    {
        yield return new WaitForSeconds(t);

        if (_ctx != null && _ctx.stats != null && _stacks > 0)
        {
            _ctx.stats.AddModifier(stat, -flat * _stacks, -percent * _stacks);
            _stacks = 0;
        }

        _timer = null;
    }

    private IEnumerator RevertOneAfter(float t)
    {
        yield return new WaitForSeconds(t);

        if (_ctx != null && _ctx.stats != null && _stacks > 0)
        {
            _ctx.stats.AddModifier(stat, -flat, -percent);
            _stacks = Mathf.Max(0, _stacks - 1);
        }
    }

    public override string Summary()
    {
        // 반드시 모든 경로에서 문자열을 반환해야 "코드 경로 중 일부만 값을 반환" 오류가 나지 않습니다.
        string amount = "";

        if (flat != 0 && Math.Abs(percent) > 0.0001f)
        {
            amount = $"+{flat} / +{percent:P0}";
        }
        else if (flat != 0)
        {
            amount = $"+{flat}";
        }
        else if (Math.Abs(percent) > 0.0001f)
        {
            amount = $"+{percent:P0}";
        }
        else
        {
            amount = "+0";
        }

        string trig = onSkillUsed ? "after skill" : (onEquip ? "on equip" : "manual");
        string dur = duration > 0f ? $"{duration:0.#}s" : "permanent";
        string pol = stacking.ToString();

        return $"{trig}: {stat} {amount} ({dur}, {pol})";
    }
}

/// <summary>
/// 코루틴 실행을 위한 보조 컴포넌트. (빈 MonoBehaviour)
/// </summary>
public class BuffHost : MonoBehaviour { }
