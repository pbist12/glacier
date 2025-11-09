using System;
using UnityEngine;

[Serializable]
public class StatModifierEffect : ItemEffect
{
    public StatType stat;
    public int flat;
    public float percent;
    public bool revertOnRemove = true;

    public override void Apply(ItemContext ctx)
    {
        ctx?.stats?.AddModifier(stat, flat, percent);
    }

    public override void Remove(ItemContext ctx)
    {
        if (revertOnRemove)
            ctx?.stats?.AddModifier(stat, -flat, -percent);
    }

    public override string Summary()
    {
        string amt = "";
        bool hasFlat = flat != 0;
        bool hasPct = Mathf.Abs(percent) > 0.0001f;

        if (hasFlat && hasPct) amt = $"+{flat} / {percent:+0%;-0%}";
        else if (hasFlat) amt = $"+{flat}";
        else if (hasPct) amt = $"{percent:+0%;-0%}";
        else amt = "+0";

        return $"(legacy) {stat} {amt} (permanent)";
    }
}
