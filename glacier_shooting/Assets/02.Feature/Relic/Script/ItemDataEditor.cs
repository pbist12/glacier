// Assets/Editor/ItemDataEditor.cs
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RelicData))]
public class ItemDataEditor : Editor
{
    bool showEffectSummaries = true;

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var item = (RelicData)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Quick Add Effects", EditorStyles.boldLabel);

        if (GUILayout.Button("+ StatModifier"))
            AddEffect(item, new StatModifierEffect { stat = StatType.FireRate, flat = 1, percent = 0f });

        if (GUILayout.Button("+ CustomFlag"))
            AddEffect(item, new CustomFlagEffect { key = "double_drop", value = 1f });

        // 🔽 여기에 네가 추가한 새 이펙트들 버튼을 이어서
        if (GUILayout.Button("+ TimedStat"))
            AddEffect(item, new TimedStatEffect { stat = StatType.BulletDamage, percent = 0.15f, duration = 5f, stacking = TimedStatEffect.StackPolicy.Refresh, onEquip = false });
        
        if (GUILayout.Button("+ NthAttack Style & Buff (5th: red +50% dmg)"))
        {
            AddEffect(item, new NthAttackStyleAndBuffEffect
            {
                nth = 5,
                // 비주얼
                tintNth = true,
                tintColor = Color.red,
                // 버프(한 발에만 적용되는 배수들)
                damageBonusPercent = 0.5f,  // +50% 데미지
                sizeMul = 1f,
                speedMul = 1f,
                lifetimeMul = 1f,
            });
        }

        if (GUILayout.Button("+ ProcOnEvent (OnHit crit x2 20%)"))
            AddEffect(item, new ProcOnEventEffect { trigger = ProcOnEventEffect.SimpleTrigger.OnHit, chance = 0.2f, icd = 0.3f, crit2x = true });

        // 요약 표시
        EditorGUILayout.Space(10);
        showEffectSummaries = EditorGUILayout.Foldout(showEffectSummaries, "Effect Summaries");
        if (showEffectSummaries && item.effects != null)
        {
            using (new EditorGUI.IndentLevelScope())
            {
                for (int i = 0; i < item.effects.Count; i++)
                {
                    var eff = item.effects[i];
                    string line = eff == null ? "(null)" : $"[{i}] {eff.GetType().Name}: {eff.Summary()}";
                    EditorGUILayout.LabelField(line);
                }
            }
        }
    }

    private void AddEffect(RelicData item, ItemEffect effect)
    {
        Undo.RecordObject(item, "Add Effect");
        if (item.effects == null) item.effects = new System.Collections.Generic.List<ItemEffect>();
        item.effects.Add(effect);
        EditorUtility.SetDirty(item);
    }
}
#endif
