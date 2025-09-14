using System.Collections.Generic;
using UnityEngine;

public static class ItemRuntime
{
    // 장착/패시브 발동
    public static void Activate(ItemData data, ItemContext ctx, List<ItemEffect> appliedCache = null)
    {
        if (data == null) return;
        foreach (var eff in data.effects)
        {
            eff?.Apply(ctx);
            appliedCache?.Add(eff);
        }
    }

    // 장착 해제/패시브 비활성화
    public static void Deactivate(ItemData data, ItemContext ctx, List<ItemEffect> appliedCache = null)
    {
        if (data == null) return;
        var list = appliedCache ?? data.effects;
        for (int i = list.Count - 1; i >= 0; i--)
        {
            list[i]?.Remove(ctx);
        }
    }

    // 사용형(OnUse) 아이템 실행
    public static bool Use(ItemData data, ItemContext ctx)
    {
        if (data == null || data.useMode != UseMode.OnUse) return false;
        foreach (var eff in data.effects)
        {
            eff?.Apply(ctx);
        }
        return true;
    }
}
