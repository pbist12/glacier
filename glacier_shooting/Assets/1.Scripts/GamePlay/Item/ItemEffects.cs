using System;
using UnityEngine;

#region === 실행 컨텍스트 ===
/*
 * 이펙트 실행에 필요한 레퍼런스를 한데 모은 바구니.
 * - owner: 아이템을 사용하는 주체(플레이어 GameObject 등)
 * - inventory: 골드/아이템 지급/회수에 사용
 * - stats: 스탯 변경/회복에 사용 (아래에 간단한 예시 인터페이스 제공)
 * - onLog: 디버그/토스트 출력 훅
 *
 * 프로젝트에 맞게 필드를 더 추가/제거해도 됩니다.
 */
public class ItemContext
{
    public GameObject owner;
    public PlayerInventory inventory;
    public IPlayerStats stats;
    public Action<string> onLog;

    public ItemContext(GameObject owner, PlayerInventory inventory = null, IPlayerStats stats = null, Action<string> logger = null)
    {
        this.owner = owner;
        this.inventory = inventory;
        this.stats = stats;
        this.onLog = logger;
    }
}
#endregion

#region === 스탯 인터페이스(예시) ===
/*
 * 프로젝트에 이미 PlayerStats가 있다면, 아래 인터페이스에 맞춰
 * 래퍼를 만들거나, IPlayerStats를 PlayerStats가 직접 구현하게 해주세요.
 *
 * 예시:
 *  - AddModifier(StatType, flat, percent): 수정치 적용(+고정, +퍼센트)
 *  - Heal(amount): 즉시 회복
 */
public enum StatType
{
    MaxHP,
    FireRate,
    BulletSpeed,
    BulletLifetime,
    BulletDamage,
    MoveSpeed,
    FocusSpeed
}

public interface IPlayerStats
{
    void AddModifier(StatType stat, int flatDelta, float percentDelta);
    void Heal(int amount);
}
#endregion

#region === 공통 인터페이스 / 베이스 ===
public interface IItemEffect
{
    void Apply(ItemContext ctx);   // 효과 적용
    void Remove(ItemContext ctx);  // 효과 해제(패시브/장비 해제 등)
    string Summary();              // 툴팁용 요약 문자열
}

[Serializable]
public abstract class ItemEffect : IItemEffect
{
    [TextArea, Tooltip("디자이너 메모(선택)")]
    public string note;

    public abstract void Apply(ItemContext ctx);

    // 필요 없으면 비워도 됨
    public virtual void Remove(ItemContext ctx) { }

    public abstract string Summary();

    protected void Log(ItemContext ctx, string msg)
    {
        if (ctx != null && ctx.onLog != null) ctx.onLog.Invoke(msg);
        else Debug.Log(msg);
    }
}
#endregion

#region === 이펙트 구현들 ===

/// <summary>
/// 스탯 수정(고정치/퍼센트). 해제 시 되돌리기 옵션 제공.
/// </summary>
[Serializable]
public class StatModifierEffect : ItemEffect
{
    [Header("스탯 수정")]
    public StatType stat;
    [Tooltip("+고정 수치")] public int flat = 0;
    [Tooltip("+퍼센트 (예: 0.15 = +15%)")][Range(-5f, 5f)] public float percent = 0f;

    [Header("해제 옵션")]
    [Tooltip("Remove 호출 시 적용분을 되돌립니다.")]
    public bool revertOnRemove = true;

    public override void Apply(ItemContext ctx)
    {
        if (ctx?.stats == null) return;
        ctx.stats.AddModifier(stat, flat, percent);
        Log(ctx, $"[{stat}] +{flat}, +{percent:P0}");
    }

    public override void Remove(ItemContext ctx)
    {
        if (!revertOnRemove || ctx?.stats == null) return;
        ctx.stats.AddModifier(stat, -flat, -percent);
        Log(ctx, $"[{stat}] revert -{flat}, -{percent:P0}");
    }

    public override string Summary()
    {
        string p = percent != 0 ? $" +{percent:P0}" : "";
        return $"+{flat}{p} {stat}";
    }
}

/// <summary>
/// 즉시 힐.
/// </summary>
[Serializable]
public class HealInstantEffect : ItemEffect
{
    [Min(1)] public int amount = 30;

    public override void Apply(ItemContext ctx)
    {
        if (ctx?.stats == null) return;
        ctx.stats.Heal(amount);
        Log(ctx, $"Heal +{amount}");
    }

    public override string Summary() => $"Heal +{amount}";
}

/// <summary>
/// 골드 지급.
/// </summary>
[Serializable]
public class AddGoldEffect : ItemEffect
{
    public int amount = 10;

    public override void Apply(ItemContext ctx)
    {
        if (ctx?.inventory == null) return;
        ctx.inventory.gold += amount;
        Log(ctx, $"Gold +{amount} (Total: {ctx.inventory.gold})");
    }

    public override string Summary() => $"Gold +{amount}";
}

/// <summary>
/// 아이템 지급. (ItemData SO를 참조)
/// </summary>
[Serializable]
public class GiveItemEffect : ItemEffect
{
    public ItemData item;      // 프로젝트의 ItemData SO
    [Min(1)] public int amount = 1;

    public override void Apply(ItemContext ctx)
    {
        if (ctx?.inventory == null || item == null) return;
        ctx.inventory.Add(item, amount);
        Log(ctx, $"Give {item.itemName} x{amount}");
    }

    public override string Summary() => $"Get {item?.itemName ?? "Unknown"} x{amount}";
}

/// <summary>
/// 커스텀 키-값 플래그. 게임 로직에서 자유롭게 해석.
/// 예: key=double_drop, value=1 -> 드랍 2배 on
/// </summary>
[Serializable]
public class CustomFlagEffect : ItemEffect
{
    public string key = "flag_key";
    public float value = 1f;

    public override void Apply(ItemContext ctx)
    {
        // 예) ctx.stats?.SetFlag(key, value);
        Log(ctx, $"Flag {key}={value}");
    }

    public override string Summary() => $"{key}: {value}";
}

#endregion

/*
 * -------------------------------------------------------------
 * 사용 가이드:
 * 1) ItemData에 [SerializeReference] List<ItemEffect> effects 가 있다고 가정.
 * 2) 인스펙터에서 effects 요소 추가 시
 *    - Add Managed Reference → 클래스명 입력 (예: StatModifierEffect)
 *    - Odin/Drawer 사용하면 타입 선택이 쉬워집니다.
 * 3) 런타임:
 *    - OnUse: foreach (var e in item.effects) e.Apply(ctx);
 *    - OnEquip/Passive 활성: 동일
 *    - OnUnequip/비활성: foreach (var e in item.effects) e.Remove(ctx);
 *
 * 필요 시 이 파일에 새로운 이펙트 클래스를 복붙해 확장하세요.
 * SO 타입은 전혀 늘어나지 않습니다.
 * -------------------------------------------------------------
 */
