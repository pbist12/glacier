// Assets/Editor/ItemDataEditor.cs
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ItemData))]
public class ItemDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var item = (ItemData)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Quick Add Effects", EditorStyles.boldLabel);

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("+ StatModifier"))
            {
                AddEffect(item, new StatModifierEffect { stat = StatType.FireRate, flat = 1, percent = 0f });
            }
            if (GUILayout.Button("+ Heal"))
            {
                AddEffect(item, new HealInstantEffect { amount = 30 });
            }
        }

        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("+ AddGold"))
            {
                AddEffect(item, new AddGoldEffect { amount = 10 });
            }
            if (GUILayout.Button("+ GiveItem"))
            {
                AddEffect(item, new GiveItemEffect { amount = 1 /* item은 수동으로 채워주세요 */ });
            }
        }

        if (GUILayout.Button("+ CustomFlag"))
        {
            AddEffect(item, new CustomFlagEffect { key = "double_drop", value = 1f });
        }
    }

    private void AddEffect(ItemData item, ItemEffect effect)
    {
        Undo.RecordObject(item, "Add Effect");
        if (item.effects == null) item.effects = new System.Collections.Generic.List<ItemEffect>();
        item.effects.Add(effect);
        EditorUtility.SetDirty(item);
    }
}
#endif
