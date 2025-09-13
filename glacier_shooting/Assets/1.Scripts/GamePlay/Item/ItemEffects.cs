using System;
using UnityEngine;

#region === ���� ���ؽ�Ʈ ===
/*
 * ����Ʈ ���࿡ �ʿ��� ���۷����� �ѵ� ���� �ٱ���.
 * - owner: �������� ����ϴ� ��ü(�÷��̾� GameObject ��)
 * - inventory: ���/������ ����/ȸ���� ���
 * - stats: ���� ����/ȸ���� ��� (�Ʒ��� ������ ���� �������̽� ����)
 * - onLog: �����/�佺Ʈ ��� ��
 *
 * ������Ʈ�� �°� �ʵ带 �� �߰�/�����ص� �˴ϴ�.
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

#region === ���� �������̽�(����) ===
/*
 * ������Ʈ�� �̹� PlayerStats�� �ִٸ�, �Ʒ� �������̽��� ����
 * ���۸� ����ų�, IPlayerStats�� PlayerStats�� ���� �����ϰ� ���ּ���.
 *
 * ����:
 *  - AddModifier(StatType, flat, percent): ����ġ ����(+����, +�ۼ�Ʈ)
 *  - Heal(amount): ��� ȸ��
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

#region === ���� �������̽� / ���̽� ===
public interface IItemEffect
{
    void Apply(ItemContext ctx);   // ȿ�� ����
    void Remove(ItemContext ctx);  // ȿ�� ����(�нú�/��� ���� ��)
    string Summary();              // ������ ��� ���ڿ�
}

[Serializable]
public abstract class ItemEffect : IItemEffect
{
    [TextArea, Tooltip("�����̳� �޸�(����)")]
    public string note;

    public abstract void Apply(ItemContext ctx);

    // �ʿ� ������ ����� ��
    public virtual void Remove(ItemContext ctx) { }

    public abstract string Summary();

    protected void Log(ItemContext ctx, string msg)
    {
        if (ctx != null && ctx.onLog != null) ctx.onLog.Invoke(msg);
        else Debug.Log(msg);
    }
}
#endregion

#region === ����Ʈ ������ ===

/// <summary>
/// ���� ����(����ġ/�ۼ�Ʈ). ���� �� �ǵ����� �ɼ� ����.
/// </summary>
[Serializable]
public class StatModifierEffect : ItemEffect
{
    [Header("���� ����")]
    public StatType stat;
    [Tooltip("+���� ��ġ")] public int flat = 0;
    [Tooltip("+�ۼ�Ʈ (��: 0.15 = +15%)")][Range(-5f, 5f)] public float percent = 0f;

    [Header("���� �ɼ�")]
    [Tooltip("Remove ȣ�� �� ������� �ǵ����ϴ�.")]
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
/// ��� ��.
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
/// ��� ����.
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
/// ������ ����. (ItemData SO�� ����)
/// </summary>
[Serializable]
public class GiveItemEffect : ItemEffect
{
    public ItemData item;      // ������Ʈ�� ItemData SO
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
/// Ŀ���� Ű-�� �÷���. ���� �������� �����Ӱ� �ؼ�.
/// ��: key=double_drop, value=1 -> ��� 2�� on
/// </summary>
[Serializable]
public class CustomFlagEffect : ItemEffect
{
    public string key = "flag_key";
    public float value = 1f;

    public override void Apply(ItemContext ctx)
    {
        // ��) ctx.stats?.SetFlag(key, value);
        Log(ctx, $"Flag {key}={value}");
    }

    public override string Summary() => $"{key}: {value}";
}

#endregion

/*
 * -------------------------------------------------------------
 * ��� ���̵�:
 * 1) ItemData�� [SerializeReference] List<ItemEffect> effects �� �ִٰ� ����.
 * 2) �ν����Ϳ��� effects ��� �߰� ��
 *    - Add Managed Reference �� Ŭ������ �Է� (��: StatModifierEffect)
 *    - Odin/Drawer ����ϸ� Ÿ�� ������ �������ϴ�.
 * 3) ��Ÿ��:
 *    - OnUse: foreach (var e in item.effects) e.Apply(ctx);
 *    - OnEquip/Passive Ȱ��: ����
 *    - OnUnequip/��Ȱ��: foreach (var e in item.effects) e.Remove(ctx);
 *
 * �ʿ� �� �� ���Ͽ� ���ο� ����Ʈ Ŭ������ ������ Ȯ���ϼ���.
 * SO Ÿ���� ���� �þ�� �ʽ��ϴ�.
 * -------------------------------------------------------------
 */
