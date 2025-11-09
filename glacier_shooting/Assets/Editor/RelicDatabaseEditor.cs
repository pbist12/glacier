#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RelicDatabase))]
public class RelicDatabaseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        var db = (RelicDatabase)target;
        GUILayout.Space(8);
        if (GUILayout.Button("Refresh From Folder"))
        {
            RelicDatabaseEditorUtil.Refresh(db);
        }
    }
}
#endif
