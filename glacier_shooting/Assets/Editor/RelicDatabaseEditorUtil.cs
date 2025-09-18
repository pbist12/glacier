#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public static class RelicDatabaseEditorUtil
{
    public static void Refresh(RelicDatabase db)
    {
        if (db == null)
        {
            Debug.LogError("RelicDatabase is null.");
            return;
        }
        if (string.IsNullOrEmpty(db.folderPath))
        {
            Debug.LogError("RelicDatabase.folderPath is empty.");
            return;
        }

        // folderPath 하위에서 타입이 RelicData인 모든 에셋 GUID 수집
        string[] guids = AssetDatabase.FindAssets("t:RelicData", new[] { db.folderPath });
        var list = new List<RelicData>(guids.Length);

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<RelicData>(path);
            if (asset != null) list.Add(asset);
        }

        db.relics = list;
        EditorUtility.SetDirty(db);
        AssetDatabase.SaveAssets();

        Debug.Log($"[RelicDatabase] Refreshed. Found {db.relics.Count} relic(s) under: {db.folderPath}");
    }
}
#endif
