using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[CreateAssetMenu(fileName = "RelicDatabase", menuName = "Relic/Relic Database")]
public class RelicDatabase : ScriptableObject
{
    [Header("Editor Scan Settings (�����Ϳ����� ���)")]
    [Tooltip("��: Assets/Game/Data/Relics")]
    public string folderPath = "Assets/Game/Data/Relics";

    [Header("Collected Relics (�ڵ� ���� ���)")]
    public List<RelicData> relics = new();

    // --- ��Ÿ�� ���� ---
    public int Count => relics?.Count ?? 0;
    public RelicData GetByIndex(int index) =>
        (index >= 0 && index < Count) ? relics[index] : null;

#if UNITY_EDITOR
    // �ν����� ��ư ���̵� ��Ŭ�� ContextMenu�� ���� ����
    [ContextMenu("Refresh From Folder")]
    public void RefreshFromFolder()
    {
        if (string.IsNullOrEmpty(folderPath))
        {
            Debug.LogError("[RelicDatabase] folderPath is empty.");
            return;
        }

        // ���� ���� ������ ��� RelicData ���� GUID ����
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
    // �÷��̾� ����/��Ÿ�ӿ����� ȣ����� �ʰ� ���
    public void RefreshFromFolder()
    {
        Debug.LogWarning("[RelicDatabase] RefreshFromFolder is editor-only.");
    }
#endif
}
