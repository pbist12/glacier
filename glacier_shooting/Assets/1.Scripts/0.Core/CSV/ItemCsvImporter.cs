#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

// 네 프로젝트의 타입 경로 맞추기
// using YourGame.Items;

public class ItemCsvImporter : EditorWindow
{
    [Header("CSV 소스")]
    public TextAsset csvFile;

    [Header("생성/업데이트 대상 폴더(프로젝트 내)")]
    public DefaultAsset outputFolder;

    [Header("리소스 로드(아이콘)")]
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
                "CSV 열 필요: ID,RelicName,Description,Rarity,Price,Cooldown,Effects,IconPath\n" +
                "IconPath는 Resources 기준 경로(확장자 제외). Effects는 'Type:...;Type:...' 규칙.",
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
        if (csvFile == null) throw new Exception("CSV 파일을 지정하세요.");
        if (outputFolder == null) throw new Exception("Output 폴더를 지정하세요.");

        var folderPath = AssetDatabase.GetAssetPath(outputFolder);
        if (string.IsNullOrEmpty(folderPath) || !AssetDatabase.IsValidFolder(folderPath))
            throw new Exception("Output Folder 경로가 올바르지 않습니다.");

        var rows = LightCsv.Parse(csvFile.text);
        if (rows.Count == 0) throw new Exception("CSV에 데이터가 없습니다.");

        // 헤더 인덱스 확인
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
            throw new Exception("헤더가 누락되었습니다. 필요한 열: ID, RelicName, Description, Rarity, Price, Cooldown, Effects, IconPath");

        var ci = CultureInfo.InvariantCulture;
        int created = 0, updated = 0;

        // 본문
        for (int r = 1; r < rows.Count; r++)
        {
            var row = rows[r];
            if (row.Length == 0) continue;

            string id = Safe(row, iID);
            if (string.IsNullOrWhiteSpace(id))
            {
                Debug.LogWarning($"[{r}] ID 없음: 건너뜀");
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
                else if (clearMissingIconsToNull) data.icon = null; // 못 찾으면 비움(옵션)
            }

            EditorUtility.SetDirty(data);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("CSV Import 완료", $"생성: {created}, 업데이트: {updated}", "OK");
    }

    static string Safe(string[] row, int i) => i >= 0 && i < row.Length ? row[i] : "";

    static int SafeInt(string[] row, int i, IFormatProvider ci, int def)
        => int.TryParse(Safe(row, i), System.Globalization.NumberStyles.Integer, ci, out var v) ? v : def;

    static float SafeFloat(string[] row, int i, IFormatProvider ci, float def)
        => float.TryParse(Safe(row, i), System.Globalization.NumberStyles.Float, ci, out var v) ? v : def;
}
#endif
