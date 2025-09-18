using System;
using System.Globalization;
using System.Collections.Generic;
using UnityEngine;

// 네가 가진 타입들 네임스페이스에 맞춰 using 조정
// using YourGame.Items;

public static class EffectFactory
{
    // ; 로 분리된 이펙트 묶음 문자열 → List<ItemEffect>
    public static List<ItemEffect> ParseEffects(string effectsField)
    {
        var list = new List<ItemEffect>();
        if (string.IsNullOrWhiteSpace(effectsField)) return list;

        string[] chunks = effectsField.Split(';');
        foreach (var raw in chunks)
        {
            var token = raw.Trim();
            if (string.IsNullOrEmpty(token)) continue;
            var eff = ParseOne(token);
            if (eff != null) list.Add(eff);
        }
        return list;
    }

    // 한 개 이펙트 "Type:..." → ItemEffect
    static ItemEffect ParseOne(string token)
    {
        // 문화권에 따라 소수점 문제가 생기지 않도록 InvariantCulture 사용
        var ci = CultureInfo.InvariantCulture;
        string[] p = token.Split(':');
        if (p.Length == 0) return null;

        switch (p[0])
        {
            case "StatModifier":
                // StatModifier:StatType:Flat:int:Percent:float
                if (p.Length < 4) { Debug.LogWarning($"StatModifier 구문 오류: {token}"); return null; }
                var e = new StatModifierEffect();
                if (Enum.TryParse(p[1], out StatType st) == false) { Debug.LogWarning($"알 수 없는 StatType: {p[1]}"); return null; }
                e.stat = st;
                if (!int.TryParse(p[2], NumberStyles.Integer, ci, out e.flat)) e.flat = 0;
                if (!float.TryParse(p[3], NumberStyles.Float, ci, out e.percent)) e.percent = 0f;
                e.revertOnRemove = true;
                return e;

            case "CustomFlag":
                // CustomFlag:key:string:value:float
                if (p.Length < 3) { Debug.LogWarning($"CustomFlag 구문 오류: {token}"); return null; }
                return new CustomFlagEffect { key = p[1], value = SafeFloat(p[2], ci) };

                // 여기에 새로운 효과 타입을 자유롭게 추가:
                // case "Heal": ...
        }
        Debug.LogWarning($"알 수 없는 이펙트 타입: {p[0]}");
        return null;
    }

    static float SafeFloat(string s, IFormatProvider ci)
        => float.TryParse(s, NumberStyles.Float, ci, out var v) ? v : 0f;
}
