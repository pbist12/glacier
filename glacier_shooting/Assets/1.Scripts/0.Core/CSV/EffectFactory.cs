using System;
using System.Globalization;
using System.Collections.Generic;
using UnityEngine;

// �װ� ���� Ÿ�Ե� ���ӽ����̽��� ���� using ����
// using YourGame.Items;

public static class EffectFactory
{
    // ; �� �и��� ����Ʈ ���� ���ڿ� �� List<ItemEffect>
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

    // �� �� ����Ʈ "Type:..." �� ItemEffect
    static ItemEffect ParseOne(string token)
    {
        // ��ȭ�ǿ� ���� �Ҽ��� ������ ������ �ʵ��� InvariantCulture ���
        var ci = CultureInfo.InvariantCulture;
        string[] p = token.Split(':');
        if (p.Length == 0) return null;

        switch (p[0])
        {
            case "StatModifier":
                // StatModifier:StatType:Flat:int:Percent:float
                if (p.Length < 4) { Debug.LogWarning($"StatModifier ���� ����: {token}"); return null; }
                var e = new StatModifierEffect();
                if (Enum.TryParse(p[1], out StatType st) == false) { Debug.LogWarning($"�� �� ���� StatType: {p[1]}"); return null; }
                e.stat = st;
                if (!int.TryParse(p[2], NumberStyles.Integer, ci, out e.flat)) e.flat = 0;
                if (!float.TryParse(p[3], NumberStyles.Float, ci, out e.percent)) e.percent = 0f;
                e.revertOnRemove = true;
                return e;

            case "CustomFlag":
                // CustomFlag:key:string:value:float
                if (p.Length < 3) { Debug.LogWarning($"CustomFlag ���� ����: {token}"); return null; }
                return new CustomFlagEffect { key = p[1], value = SafeFloat(p[2], ci) };

                // ���⿡ ���ο� ȿ�� Ÿ���� �����Ӱ� �߰�:
                // case "Heal": ...
        }
        Debug.LogWarning($"�� �� ���� ����Ʈ Ÿ��: {p[0]}");
        return null;
    }

    static float SafeFloat(string s, IFormatProvider ci)
        => float.TryParse(s, NumberStyles.Float, ci, out var v) ? v : 0f;
}
