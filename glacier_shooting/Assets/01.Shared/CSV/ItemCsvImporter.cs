#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// �� ������Ʈ�� Ÿ�� ��� ���߱�
// using YourGame.Items;

public class ItemCsvImporter : EditorWindow
{
    [Header("CSV �ҽ�")]
    public TextAsset csvFile;

    [Header("����/������Ʈ ��� ����(������Ʈ ��)")]
    public DefaultAsset outputFolder;

    [Header("���ҽ� �ε�(������)")]
    public bool useResourcesForIcon = true;  // Resources.Load<Sprite>(IconPath)
    public bool clearMissingIconsToNull = true;

    Vector2 _scroll;
    const string MENU = "Tools/CSV/Import ItemData (Relics)";

    [MenuItem(MENU)]
    public static void Open()
    {
        var w = GetWindow<ItemCsvImporter>(true, "Item CSV Importer");
        w.minSize = new Vector2(480, 300);
    }

    void OnGUI()
    {
        using (var s = new EditorGUILayout.ScrollViewScope(_scroll))
        {
            _scroll = s.scrollPosition;

            EditorGUILayout.HelpBox(
                "CSV �� �ʿ�: ID,RelicName,Description,Rarity,Price,Cooldown,Effects,IconPath\n" +
                "IconPath�� Resources ���� ���(Ȯ���� ����). Effects�� 'Type:...;Type:...' ��Ģ.",
                MessageType.Info);

            csvFile = (TextAsset)EditorGUILayout.ObjectField("CSV File", csvFile, typeof(TextAsset), false);
            outputFolder = (DefaultAsset)EditorGUILayout.ObjectField("Output Folder", outputFolder, typeof(DefaultAsset), false);

            useResourcesForIcon = EditorGUILayout.Toggle("Use Resources for Icon", useResourcesForIcon);
            clearMissingIconsToNull = EditorGUILayout.Toggle("Clear Missing Icons To Null", clearMissingIconsToNull);

            GUILayout.Space(8);
            if (GUILayout.Button("Import / Update"))
            {
                try
                {
                    Import();
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex);
                    EditorUtility.DisplayDialog("Import Error", ex.Message, "OK");
                }
            }
        }
    }

    void Import()
    {
        if (csvFile == null) throw new Exception("CSV ������ �����ϼ���.");
        if (outputFolder == null) throw new Exception("Output ������ �����ϼ���.");

        var folderPath = AssetDatabase.GetAssetPath(outputFolder);
        if (string.IsNullOrEmpty(folderPath) || !AssetDatabase.IsValidFolder(folderPath))
            throw new Exception("Output Folder ��ΰ� �ùٸ��� �ʽ��ϴ�.");

        var rows = LightCsv.Parse(csvFile.text);
        if (rows.Count == 0) throw new Exception("CSV�� �����Ͱ� �����ϴ�.");

        // ��� �ε��� Ȯ��
        var header = rows[0];
        int iID = Array.IndexOf(header, "ID");
        int iName = Array.IndexOf(header, "RelicName");
        int iDesc = Array.IndexOf(header, "Description");
        int iRarity = Array.IndexOf(header, "Rarity");
        int iPrice = Array.IndexOf(header, "Price");
        int iCooldown = Array.IndexOf(header, "Cooldown");
        int iEffects = Array.IndexOf(header, "Effects");
        int iIcon = Array.IndexOf(header, "IconPath");

        int[] must = { iID, iName, iDesc, iRarity, iPrice, iCooldown, iEffects, iIcon };
        if (must.Any(x => x < 0))
            throw new Exception("����� �����Ǿ����ϴ�. �ʿ��� ��: ID, RelicName, Description, Rarity, Price, Cooldown, Effects, IconPath");

        var ci = CultureInfo.InvariantCulture;
        int created = 0, updated = 0;

        // ����
        for (int r = 1; r < rows.Count; r++)
        {
            var row = rows[r];
            if (row.Length == 0) continue;

            string id = Safe(row, iID);
            if (string.IsNullOrWhiteSpace(id))
            {
                Debug.LogWarning($"[{r}] ID ����: �ǳʶ�");
                continue;
            }

            string assetPath = $"{folderPath}/{id}.asset";
            var data = AssetDatabase.LoadAssetAtPath<RelicData>(assetPath);
            bool isNew = data == null;
            if (isNew)
            {
                data = ScriptableObject.CreateInstance<RelicData>();
                AssetDatabase.CreateAsset(data, assetPath);
                created++;
            }
            else updated++;

            Undo.RegisterCompleteObjectUndo(data, "CSV Import ItemData");

            data.RelicName = Safe(row, iName);
            data.description = Safe(row, iDesc);

            var rarityStr = Safe(row, iRarity);
            if (!Enum.TryParse(rarityStr, out Rarity rarity)) rarity = Rarity.Common;
            data.rarity = rarity;

            data.price = SafeInt(row, iPrice, ci, 0);
            data.cooldown = SafeFloat(row, iCooldown, ci, 0);

            // Effects
            data.effects = EffectFactory.ParseEffects(Safe(row, iEffects));

            // Icon
            var iconPath = Safe(row, iIcon);
            if (useResourcesForIcon)
            {
                Sprite spr = null;
                if (!string.IsNullOrWhiteSpace(iconPath))
                    spr = Resources.Load<Sprite>(iconPath);

                if (spr != null) data.icon = spr;
                else if (clearMissingIconsToNull) data.icon = null; // �� ã���� ���(�ɼ�)
            }

            EditorUtility.SetDirty(data);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("CSV Import �Ϸ�", $"����: {created}, ������Ʈ: {updated}", "OK");
    }

    static string Safe(string[] row, int i) => i >= 0 && i < row.Length ? row[i] : "";

    static int SafeInt(string[] row, int i, IFormatProvider ci, int def)
        => int.TryParse(Safe(row, i), System.Globalization.NumberStyles.Integer, ci, out var v) ? v : def;

    static float SafeFloat(string[] row, int i, IFormatProvider ci, float def)
        => float.TryParse(Safe(row, i), System.Globalization.NumberStyles.Float, ci, out var v) ? v : def;
}
#endif
