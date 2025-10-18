using System;
using System.Globalization;
using System.Collections.Generic;
using UnityEngine;

public static class EffectFactory
{
    public static List<ItemEffect> ParseEffects(string effectsField)
    {
        var list = new List<ItemEffect>();
        if (string.IsNullOrWhiteSpace(effectsField)) return list;

        var ci = CultureInfo.InvariantCulture;
        string[] chunks = effectsField.Split(';');
        foreach (var raw in chunks)
        {
            var token = raw.Trim();
            if (string.IsNullOrEmpty(token)) continue;
            var eff = ParseOne(token, ci);
            if (eff != null) list.Add(eff);
        }
        return list;
    }

    static ItemEffect ParseOne(string token, IFormatProvider ci)
    {
        string[] p = token.Split(':');
        if (p.Length == 0) return null;

        switch (p[0])
        {
            case "StatModifier":
                // StatModifier:StatType:Flat:int:Percent:float
                if (p.Length < 4) { Debug.LogWarning($"StatModifier 구문 오류: {token}"); return null; }
                if (!Enum.TryParse(p[1], out StatType st)) { Debug.LogWarning($"알 수 없는 StatType: {p[1]}"); return null; }

                var e = new StatModifierEffect { stat = st };
                e.flat = int.TryParse(p[2], NumberStyles.Integer, ci, out var i) ? i : 0;
                e.percent = float.TryParse(p[3], NumberStyles.Float, ci, out var f) ? f : 0f;
                e.revertOnRemove = true;
                return e;

            case "CustomFlag":
                // CustomFlag:key:value(float)
                if (p.Length < 3) { Debug.LogWarning($"CustomFlag 구문 오류: {token}"); return null; }
                return new CustomFlagEffect { key = p[1], value = float.TryParse(p[2], NumberStyles.Float, ci, out var v) ? v : 0f };

                // TODO: "TimedStat:StatType:flat:int:percent:float:duration:float" 등 필요 타입을 점진적으로 추가
        }

        Debug.LogWarning($"알 수 없는 이펙트 타입: {p[0]}");
        return null;
    }
}
