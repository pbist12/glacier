// Assets/Editor/ItemDataEditor.cs
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RelicData))]
public class ItemDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        var item = (RelicData)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Quick Add Effects", EditorStyles.boldLabel);

        if (GUILayout.Button("+ StatModifier"))
        {
            AddEffect(item, new StatModifierEffect { stat = StatType.FireRate, flat = 1, percent = 0f });
        }
        if (GUILayout.Button("+ CustomFlag"))
        {
            AddEffect(item, new CustomFlagEffect { key = "double_drop", value = 1f });
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
