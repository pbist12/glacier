#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

public class RelicCsvImporterWindow : EditorWindow
{
    // ====== UI 상태 ======
    private string csvUrl = "https://docs.google.com/spreadsheets/d/1pII91YqewQ2LJCQ-DwTCnZVtMZZPnMw6lm_VejBGvUI/export?format=csv";
    private string targetFolder = "Assets/02.Feature/Relic/SO/1.AllRelic";
    private bool dryRun = false;
    private bool autoRefreshDatabase = true;
    private Vector2 _scroll;

    // 로그/리포트
    private readonly StringBuilder _log = new StringBuilder();
    private int _created, _updated, _skipped;
    private int _effectOk, _effectFail;

    [MenuItem("Tools/Relic CSV Importer")]
    public static void ShowWindow()
    {
        GetWindow<RelicCsvImporterWindow>("Relic CSV Importer");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Relic CSV Importer", EditorStyles.boldLabel);
        EditorGUILayout.Space(4);

        csvUrl = EditorGUILayout.TextField("CSV URL", csvUrl);
        using (new EditorGUILayout.HorizontalScope())
        {
            targetFolder = EditorGUILayout.TextField("Target Folder", targetFolder);
            if (GUILayout.Button("Pick", GUILayout.Width(60)))
            {
                var picked = EditorUtility.OpenFolderPanel("Select target folder under Assets", "Assets", "");
                if (!string.IsNullOrEmpty(picked))
                {
                    if (picked.StartsWith(Application.dataPath))
                    {
                        targetFolder = "Assets" + picked.Substring(Application.dataPath.Length);
                    }
                    else
                    {
                        EditorUtility.DisplayDialog("Invalid Folder", "Pick a folder under Assets/", "OK");
                    }
                }
            }
        }

        dryRun = EditorGUILayout.ToggleLeft("Dry Run (검증만, 저장 안 함)", dryRun);
        autoRefreshDatabase = EditorGUILayout.ToggleLeft("Import 후 RelicDatabase 자동 갱신", autoRefreshDatabase);

        EditorGUILayout.Space(8);
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Fetch & Import", GUILayout.Height(30)))
            {
                ImportNow();
            }
            if (GUILayout.Button("Validate Only", GUILayout.Height(30)))
            {
                dryRun = true;
                ImportNow();
            }
        }

        EditorGUILayout.Space(8);
        EditorGUILayout.LabelField("Report", EditorStyles.boldLabel);
        _scroll = EditorGUILayout.BeginScrollView(_scroll);
        EditorGUILayout.TextArea(_log.ToString(), GUILayout.ExpandHeight(true));
        EditorGUILayout.EndScrollView();
    }

    private void ImportNow()
    {
        _log.Length = 0;
        _created = _updated = _skipped = 0;
        _effectOk = _effectFail = 0;

        try
        {
            if (string.IsNullOrWhiteSpace(csvUrl))
                throw new Exception("CSV URL 이 비어 있습니다.");

            // 1) CSV 다운로드
            string csvText = DownloadString(csvUrl);
            if (string.IsNullOrWhiteSpace(csvText))
                throw new Exception("CSV 응답이 비어있습니다.");

            // 2) CSV 파싱 → rows
            var rows = CsvHelper.Parse(csvText);
            if (rows.Count == 0)
                throw new Exception("CSV에 데이터 행이 없습니다.");

            // 3) 행 처리
            EnsureFolder(targetFolder);
            var dbToRefresh = autoRefreshDatabase ? FindRelicDatabase() : null;

            foreach (var row in rows)
            {
                ProcessRow(row);
            }

            // 4) 저장/마무리
            if (!dryRun)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            if (!dryRun && dbToRefresh != null)
            {
                try
                {
                    dbToRefresh.RefreshFromFolder();
                }
                catch (Exception e)
                {
                    LogWarn($"RelicDatabase 갱신 중 오류: {e.Message}");
                }
            }

            // 리포트
            LogInfo($"==== Import Finished ====");
            LogInfo($"Created:   {_created}");
            LogInfo($"Updated:   {_updated}");
            LogInfo($"Skipped:   {_skipped}");
            LogInfo($"Effects OK / Fail: {_effectOk} / {_effectFail}");
        }
        catch (Exception e)
        {
            LogError($"[Fatal] {e.Message}\n{e.StackTrace}");
        }
    }

    // ====== 한 행 처리 ======
    private void ProcessRow(Dictionary<string, string> row)
    {
        string Get(string k) => row.TryGetValue(k, out var v) ? v?.Trim() : null;

        var id = Get("RelicName");
        if (string.IsNullOrWhiteSpace(id))
        {
            _skipped++;
            LogWarn("행 스킵: RelicID 가 비어있음.");
            return;
        }

        // 존재 확인 or 생성
        var assetPath = $"{targetFolder}/{SanitizeFileName(id)}.asset";
        var item = AssetDatabase.LoadAssetAtPath<RelicData>(assetPath);
        bool isNew = false;
        if (item == null)
        {
            if (!dryRun)
            {
                item = ScriptableObject.CreateInstance<RelicData>();
                AssetDatabase.CreateAsset(item, assetPath);
            }
            isNew = true;
        }

        // 필드 매핑
        if (!dryRun && item != null)
        {
            Undo.RecordObject(item, isNew ? "Create Relic" : "Update Relic");
        }

        string name = Get("RelicName");
        string desc = Get("Description");
        string rarityStr = Get("Rarity");
        string priceStr = Get("Price");
        string cooldownStr = Get("Cooldown");
        string iconPath = Get("IconPath");
        string effects = Get("Effects");

        // 값 세팅
        if (!dryRun && item != null)
        {
            item.RelicID = id;
            if (!string.IsNullOrWhiteSpace(name)) item.RelicName = name;
            if (!string.IsNullOrWhiteSpace(desc)) item.description = desc;

            // Rarity
            if (!string.IsNullOrWhiteSpace(rarityStr))
            {
                if (EnumTryParseIgnoreCase<Rarity>(rarityStr, out var rar)) item.rarity = rar;
                else LogWarn($"{id}: Rarity 파싱 실패 → 값: {rarityStr}");
            }

            // Price
            if (TryParseInt(priceStr, out var price)) item.price = Math.Max(0, price);
            else if (!string.IsNullOrEmpty(priceStr)) LogWarn($"{id}: Price 파싱 실패 → 값: {priceStr}");

            // Cooldown
            if (TryParseFloat(cooldownStr, out var cd)) item.cooldown = Mathf.Max(0f, cd);
            else if (!string.IsNullOrEmpty(cooldownStr)) LogWarn($"{id}: Cooldown 파싱 실패 → 값: {cooldownStr}");

            // Icon
            if (!string.IsNullOrWhiteSpace(iconPath))
            {
                var sprite = LoadSprite(iconPath);
                if (sprite != null) item.icon = sprite;
                else LogWarn($"{id}: IconPath 로드 실패 → {iconPath}");
            }

            // Effects
            var newEffects = new List<ItemEffect>();
            if (!string.IsNullOrWhiteSpace(effects))
            {
                foreach (var token in EffectDsl.SplitEffects(effects))
                {
                    if (string.IsNullOrWhiteSpace(token)) continue;

                    if (!EffectDsl.ParseEffectToken(token, out var typeName, out var args))
                    {
                        _effectFail++;
                        LogWarn($"{id}: 이펙트 토큰 파싱 실패 → {token}");
                        continue;
                    }

                    var effect = EffectFactory.Create(typeName);
                    if (effect == null)
                    {
                        _effectFail++;
                        LogWarn($"{id}: 미지원 이펙트 타입 → {typeName}");
                        continue;
                    }

                    // 인자 바인딩
                    if (!ParamBinder.TryBind(effect, args, out var bindErr))
                    {
                        _effectFail++;
                        LogWarn($"{id}: 이펙트 파라미터 바인딩 실패 → {typeName} ({bindErr})");
                        continue;
                    }

                    _effectOk++;
                    newEffects.Add(effect);
                }
            }

            item.effects = newEffects;
            EditorUtility.SetDirty(item);
        }

        if (isNew) _created++;
        else _updated++;

        if (dryRun)
        {
            LogInfo($"[{id}] {(isNew ? "WouldCreate" : "WouldUpdate")}  name='{name}' rarity='{rarityStr}' price='{priceStr}' effects={CountEffects(effects)}");
        }
        else
        {
            LogInfo($"[{id}] {(isNew ? "Created" : "Updated")}  name='{name}' rarity='{rarityStr}' price='{priceStr}' effects={CountEffects(effects)}");
        }
    }

    // ====== 유틸 ======
    private static int CountEffects(string effects)
    {
        if (string.IsNullOrWhiteSpace(effects)) return 0;
        return EffectDsl.SplitEffects(effects).Count(t => !string.IsNullOrWhiteSpace(t));
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;

        var parts = path.Split('/');
        var sb = new StringBuilder(parts[0]);
        for (int i = 1; i < parts.Length; i++)
        {
            var parent = sb.ToString();
            var child = parts[i];
            if (!AssetDatabase.IsValidFolder(parent + "/" + child))
                AssetDatabase.CreateFolder(parent, child);
            sb.Append("/").Append(child);
        }
    }

    private static string SanitizeFileName(string s)
    {
        foreach (var c in Path.GetInvalidFileNameChars())
            s = s.Replace(c, '_');
        return s;
    }

    private static RelicDatabase FindRelicDatabase()
    {
        var guids = AssetDatabase.FindAssets("t:RelicDatabase");
        if (guids != null && guids.Length > 0)
        {
            var path = AssetDatabase.GUIDToAssetPath(guids[0]);
            return AssetDatabase.LoadAssetAtPath<RelicDatabase>(path);
        }
        return null;
    }

    private static string DownloadString(string url)
    {
        // 에디터 환경에서는 WebClient로 간단히
        try
        {
            using (var wc = new System.Net.WebClient())
            {
                wc.Encoding = Encoding.UTF8;
                return wc.DownloadString(url);
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"CSV 다운로드 실패: {ex.Message}");
        }
    }

    private static Sprite LoadSprite(string pathOrResource)
    {
        // Assets 경로
        if (pathOrResource.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
        {
            var sp = AssetDatabase.LoadAssetAtPath<Sprite>(pathOrResource);
            return sp;
        }
        // Resources 로드
        var sp2 = Resources.Load<Sprite>(pathOrResource);
        return sp2;
    }

    private static bool TryParseInt(string s, out int v)
    {
        if (string.IsNullOrWhiteSpace(s)) { v = 0; return false; }
        return int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out v);
    }

    private static bool TryParseFloat(string s, out float v)
    {
        if (string.IsNullOrWhiteSpace(s)) { v = 0f; return false; }
        return float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out v);
    }

    private static bool EnumTryParseIgnoreCase<T>(string s, out T v) where T : struct
        => Enum.TryParse<T>(s, true, out v);

    private void LogInfo(string msg) => _log.AppendLine(msg);
    private void LogWarn(string msg) => _log.AppendLine("⚠ " + msg);
    private void LogError(string msg) => _log.AppendLine("❌ " + msg);
}

// ===================== CSV Helper =====================
static class CsvHelper
{
    // CSV → List<Dictionary<header, value>>  (따옴표/콤마 처리)
    public static List<Dictionary<string, string>> Parse(string text)
    {
        var rows = new List<Dictionary<string, string>>();
        var reader = new StringReader(text);
        var header = ReadRow(reader);
        if (header == null || header.Count == 0) return rows;

        while (true)
        {
            var row = ReadRow(reader);
            if (row == null) break;
            if (row.Count == 0) continue;

            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            for (int i = 0; i < header.Count; i++)
            {
                var key = header[i];
                var val = i < row.Count ? row[i] : "";
                dict[key] = val;
            }
            rows.Add(dict);
        }
        return rows;
    }

    // 한 줄 읽기(개행/따옴표 포함)
    private static List<string> ReadRow(StringReader r)
    {
        var sb = new StringBuilder();
        var fields = new List<string>();
        bool inQuotes = false;
        while (true)
        {
            int ch = r.Read();
            if (ch == -1)
            {
                if (sb.Length > 0 || inQuotes || fields.Count > 0)
                {
                    fields.Add(sb.ToString());
                    return fields;
                }
                return fields.Count == 0 ? null : fields;
            }

            char c = (char)ch;
            if (c == '\"')
            {
                if (inQuotes)
                {
                    int peek = r.Peek();
                    if (peek == '\"') { r.Read(); sb.Append('\"'); } // escaped quote
                    else inQuotes = false;
                }
                else
                {
                    inQuotes = true;
                }
            }
            else if (c == ',' && !inQuotes)
            {
                fields.Add(sb.ToString());
                sb.Length = 0;
            }
            else if ((c == '\n' || c == '\r') && !inQuotes)
            {
                // consume \r\n pair
                if (c == '\r' && r.Peek() == '\n') r.Read();
                fields.Add(sb.ToString());
                return fields;
            }
            else
            {
                sb.Append(c);
            }
        }
    }
}

// ===================== Effect DSL & Factory =====================
public static class EffectDsl
{
    /// <summary>
    /// Effects 셀을 ; 기준으로 나누고, 각 토큰의 양끝 따옴표를 제거해 정리합니다.
    /// 예) "StatModifier(...); NthShot(...)" -> ["StatModifier(...)", "NthShot(...)"]
    /// </summary>
    public static IEnumerable<string> SplitEffects(string effectsCell)
    {
        if (string.IsNullOrWhiteSpace(effectsCell))
            return Array.Empty<string>();

        return effectsCell
            .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(s => UnwrapQuotes(s.Trim()))
            .Where(s => s.Length > 0);
    }

    /// <summary>
    /// "TypeName(a=1,b=2)" 형태를 파싱하여 typeName 과 args(dict)를 생성합니다.
    /// 토큰이 따옴표로 감싸져 있어도 자동 제거합니다.
    /// </summary>
    public static bool ParseEffectToken(string token, out string typeName, out Dictionary<string, string> args)
    {
        typeName = null;
        args = null;

        if (string.IsNullOrWhiteSpace(token))
            return false;

        token = UnwrapQuotes(token);

        // TypeName (paramlist) 캡처
        var m = Regex.Match(token, @"^\s*([A-Za-z_][A-Za-z0-9_]*)\s*(?:\((.*)\))?\s*$");
        if (!m.Success)
            return false;

        typeName = m.Groups[1].Value;
        args = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        if (!m.Groups[2].Success)
            return true; // 파라미터 없음

        var inner = m.Groups[2].Value;
        foreach (var part in SplitArgs(inner))
        {
            var kv = part.Split(new[] { '=' }, 2);
            if (kv.Length != 2) continue;

            var key = kv[0].Trim();
            var val = TrimQuotes(kv[1].Trim()); // 값 내부의 양끝 따옴표 제거
            if (key.Length == 0) continue;

            args[key] = val;
        }
        return true;
    }

    /// <summary>
    /// param1=1, param2="a,b", param3='x' 같은 문자열을
    /// 콤마 기준으로 안전하게 분리합니다(따옴표 내부 콤마 무시).
    /// </summary>
    private static IEnumerable<string> SplitArgs(string s)
    {
        if (string.IsNullOrEmpty(s))
            yield break;

        var sb = new StringBuilder();
        bool inDouble = false, inSingle = false, inSmart = false;

        for (int i = 0; i < s.Length; i++)
        {
            char c = s[i];

            // 따옴표 상태 토글
            if (!inSingle && !inSmart && c == '"')
            {
                inDouble = !inDouble;
                sb.Append(c);
                continue;
            }
            if (!inDouble && !inSmart && c == '\'')
            {
                inSingle = !inSingle;
                sb.Append(c);
                continue;
            }
            if (!inDouble && !inSingle && (c == '“' || c == '”'))
            {
                // 스마트쿼트는 짝이 다를 수 있어, 같은 문자로 토글
                inSmart = !inSmart;
                sb.Append(c);
                continue;
            }

            // 분리 기준: 최상위 콤마
            if (c == ',' && !inDouble && !inSingle && !inSmart)
            {
                var part = sb.ToString().Trim();
                if (part.Length > 0) yield return part;
                sb.Length = 0;
                continue;
            }

            sb.Append(c);
        }

        if (sb.Length > 0)
        {
            var last = sb.ToString().Trim();
            if (last.Length > 0) yield return last;
        }
    }

    /// <summary>
    /// 값의 양끝 따옴표를 제거합니다. "foo" / 'foo' / “foo” 모두 처리.
    /// </summary>
    private static string TrimQuotes(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return s;
        s = s.Trim();

        if (s.Length >= 2)
        {
            char first = s[0], last = s[^1];
            if ((first == '"' && last == '"') ||
                (first == '\'' && last == '\'') ||
                (first == '“' && last == '”'))
            {
                return s.Substring(1, s.Length - 2).Trim();
            }
        }
        return s;
    }

    /// <summary>
    /// 토큰 전체가 따옴표로 감싸져 있으면 제거합니다.
    /// </summary>
    private static string UnwrapQuotes(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return s;
        s = s.Trim();

        if (s.Length >= 2)
        {
            char first = s[0], last = s[^1];
            if ((first == '"' && last == '"') ||
                (first == '\'' && last == '\'') ||
                (first == '“' && last == '”'))
            {
                s = s.Substring(1, s.Length - 2).Trim();
            }
        }
        return s;
    }
}

static class EffectFactory
{
    // alias → canonical
    private static readonly Dictionary<string, string> Alias = new(StringComparer.OrdinalIgnoreCase)
    {
        // NthAttack 계열
        ["nthshot"] = "nthattack",
        ["nthattackstyleandbuff"] = "nthattack",

        // Proc 계열
        ["procevent"] = "proconevent",
        ["procon"] = "proconevent",

        // Flag 계열
        ["customflag"] = "flag",
    };

    public static ItemEffect Create(string typeName)
    {
        if (string.IsNullOrWhiteSpace(typeName)) return null;

        var key = Canonicalize(typeName);
        if (Alias.TryGetValue(key, out var mapped)) key = mapped;

        switch (key)
        {
            case "statmodifier": return new StatModifierEffect();
            case "timedstat": return new TimedStatEffect();
            case "nthattack": return new NthAttackStyleAndBuffEffect();
            case "proconevent": return new ProcOnEventEffect();
            case "flag": return new CustomFlagEffect();
            default:
                return ReflectionFallback(typeName);
        }
    }

    private static string Canonicalize(string s)
    {
        s = s.Trim().ToLowerInvariant();
        s = s.Replace(" ", "").Replace("_", "").Replace("-", "");
        if (s.EndsWith("effect")) s = s[..^"effect".Length];
        return s;
    }

    private static ItemEffect ReflectionFallback(string originalTypeName)
    {
        var t = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(SafeTypes)
            .FirstOrDefault(tt => typeof(ItemEffect).IsAssignableFrom(tt)
                               && string.Equals(tt.Name, originalTypeName, StringComparison.OrdinalIgnoreCase));
        return t != null ? Activator.CreateInstance(t) as ItemEffect : null;
    }

    private static IEnumerable<Type> SafeTypes(Assembly a)
    {
        try { return a.GetTypes(); } catch { return Array.Empty<Type>(); }
    }
}

static class ParamBinder
{
    public static bool TryBind(object target, Dictionary<string, string> args, out string error)
    {
        error = null;
        foreach (var (k, v) in args)
        {
            var member = FindField(target.GetType(), k);
            if (member == null)
            {
                // 알 수 없는 파라미터는 경고만 (실패로 취급하지 않음)
                continue;
            }

            try
            {
                object converted = ConvertValue(v, member.FieldType);
                member.SetValue(target, converted);
            }
            catch (Exception e)
            {
                error = $"{k}={v} → {e.Message}";
                return false;
            }
        }
        return true;
    }

    private static FieldInfo FindField(Type t, string name)
    {
        // 대소문자 무시, underscore도 허용
        var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy;
        return t.GetFields(flags).FirstOrDefault(f =>
            string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase) ||
            string.Equals(f.Name.Replace("_", ""), name.Replace("_", ""), StringComparison.OrdinalIgnoreCase));
    }

    private static object ConvertValue(string raw, Type targetType)
    {
        if (targetType == typeof(string)) return raw;
        if (targetType == typeof(int))
        {
            if (!int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var iv))
                throw new Exception("int 변환 실패");
            return iv;
        }
        if (targetType == typeof(float))
        {
            if (!float.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out var fv))
                throw new Exception("float 변환 실패");
            return fv;
        }
        if (targetType == typeof(bool))
        {
            if (bool.TryParse(raw, out var bv)) return bv;
            // 0/1 허용
            if (raw == "0") return false;
            if (raw == "1") return true;
            throw new Exception("bool 변환 실패");
        }
        if (targetType == typeof(Color))
        {
            if (TryParseColor(raw, out var c)) return c;
            throw new Exception("Color 변환 실패");
        }
        if (targetType.IsEnum)
        {
            if (Enum.TryParse(targetType, raw, true, out var ev)) return ev;
            throw new Exception($"enum {targetType.Name} 변환 실패");
        }

        // 배열/리스트 등은 필요 시 확장
        throw new Exception($"미지원 타입: {targetType.Name}");
    }

    private static bool TryParseColor(string s, out Color c)
    {
        // #RRGGBB / #RRGGBBAA / rgb(a)
        if (!string.IsNullOrWhiteSpace(s) && s[0] == '#')
        {
            if (ColorUtility.TryParseHtmlString(s, out c)) return true;
            c = default; return false;
        }
        if (s.StartsWith("rgba", StringComparison.OrdinalIgnoreCase) ||
            s.StartsWith("rgb", StringComparison.OrdinalIgnoreCase))
        {
            var nums = Regex.Matches(s, @"[\d\.]+").Cast<Match>().Select(m => m.Value).ToList();
            if (nums.Count >= 3)
            {
                float r = float.Parse(nums[0], CultureInfo.InvariantCulture) / 255f;
                float g = float.Parse(nums[1], CultureInfo.InvariantCulture) / 255f;
                float b = float.Parse(nums[2], CultureInfo.InvariantCulture) / 255f;
                float a = nums.Count >= 4 ? float.Parse(nums[3], CultureInfo.InvariantCulture) : 1f;
                c = new Color(r, g, b, a);
                return true;
            }
        }
        c = default;
        return false;
    }
}
#endif
