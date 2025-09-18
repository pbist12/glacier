using System;
using UnityEngine;

#region === ���� ���ؽ�Ʈ ==
/// <summary>
///  * ����Ʈ ���࿡ �ʿ��� ���۷����� �ѵ� ���� �ٱ���.
///  * - owner: �������� ����ϴ� ��ü(�÷��̾� GameObject ��)
///  * - inventory: ���/������ ����/ȸ���� ���
///  * - stats: ���� ����/ȸ���� ��� (�Ʒ��� ������ ���� �������̽� ����)
///  * - onLog: �����/�佺Ʈ ��� ��
///  * ������Ʈ�� �°� �ʵ带 �� �߰�/�����ص� �˴ϴ�.
/// </summary>

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

/// <summary>
/// ������Ʈ�� �̹� PlayerStats�� �ִٸ�, �Ʒ� �������̽��� ����
/// ���۸� ����ų�, IPlayerStats�� PlayerStats�� ���� �����ϰ� ���ּ���.
/// ����:
/// - AddModifier(StatType, flat, percent): ����ġ ����(+����, +�ۼ�Ʈ)
/// - Heal(amount): ��� ȸ��
/// </summary>

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
}
[Serializable]
public abstract class ItemEffect : IItemEffect
{
    [TextArea, Tooltip("�����̳� �޸�(����)")]
    public string note;
    public abstract void Apply(ItemContext ctx);
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
