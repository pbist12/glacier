// ItemEffect.cs  (기존 ItemEffects.cs에서 베이스/공용만 남겨도 됩니다)
using System;
using UnityEngine;

#region 실행 컨텍스트 / 스탯 인터페이스 / 열거형
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

public enum StatType
{
    MaxHP,
    MaxMP,
    FireDirection,
    FireRate,
    BulletSize,
    BulletSpeed,
    BulletLifetime,
    BulletDamage,
    MoveSpeed,
    FocusSpeed,
    MagnetRange
}
public interface IPlayerStats
{
    void AddModifier(StatType stat, int flatDelta, float percentDelta);
    void Heal(int amount);
}
#endregion

#region 베이스
public interface IItemEffect
{
    void Apply(ItemContext ctx);
    void Remove(ItemContext ctx);
}

[Serializable]
public abstract class ItemEffect : IItemEffect
{
    [TextArea, Tooltip("디자이너 메모(선택)")]
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
