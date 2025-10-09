using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "RelicDatabase", menuName = "Relic/Relic Database")]
public class RelicDatabase : ScriptableObject
{
    [Header("Editor Scan Settings (에디터에서만 사용)")]
    [Tooltip("예: Assets/Game/Data/Relics")]
    public string folderPath = "Assets/Game/Data/Relics";

    [Header("Collected Relics (자동 수집 결과)")]
    public List<RelicData> relics = new();

    // --- 런타임 헬퍼 ---
    public int Count => relics?.Count ?? 0;
    public RelicData GetByIndex(int index) =>
        (index >= 0 && index < Count) ? relics[index] : null;

#if UNITY_EDITOR
    // 인스펙터 버튼 없이도 우클릭 ContextMenu로 갱신 가능
    [ContextMenu("Refresh From Folder")]
    public void RefreshFromFolder()
    {
        if (string.IsNullOrEmpty(folderPath))
        {
            Debug.LogError("[RelicDatabase] folderPath is empty.");
            return;
        }

        // 지정 폴더 하위의 모든 RelicData 에셋 GUID 수집
        string[] guids = AssetDatabase.FindAssets("t:RelicData", new[] { folderPath });
        var list = new List<RelicData>(guids.Length);

        foreach (var guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var asset = AssetDatabase.LoadAssetAtPath<RelicData>(path);
            if (asset != null) list.Add(asset);
        }

        relics = list;
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();

        Debug.Log($"[RelicDatabase] Refreshed. Found {relics.Count} relic(s) under: {folderPath}");
    }
#else
    // 플레이어 빌드/런타임에서는 호출되지 않게 방어
    public void RefreshFromFolder()
    {
        Debug.LogWarning("[RelicDatabase] RefreshFromFolder is editor-only.");
    }
#endif
}
