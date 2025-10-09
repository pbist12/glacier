using System;
using System.Collections.Generic;
using System.Text;

public static class LightCsv
{
    // 간단/안전한 CSV 파서: 따옴표로 감싼 필드, 내부의 "" 이스케이프, 콤마 포함 필드 처리
    public static List<string[]> Parse(string csv)
    {
        var rows = new List<string[]>();
        if (string.IsNullOrEmpty(csv)) return rows;

        int i = 0;
        int len = csv.Length;
        var row = new List<string>();
        var cell = new StringBuilder();
        bool inQuotes = false;

        void EndCell() { row.Add(cell.ToString()); cell.Length = 0; }
        void EndRow()
        {
            if (inQuotes) { /*비정상 CSV 방어*/ inQuotes = false; }
            EndCell();
            rows.Add(row.ToArray());
            row.Clear();
        }

        while (i < len)
        {
            char c = csv[i++];
            if (inQuotes)
            {
                if (c == '"')
                {
                    if (i < len && csv[i] == '"') { cell.Append('"'); i++; } // "" -> "
                    else inQuotes = false;
                }
                else cell.Append(c);
            }
            else
            {
                if (c == '"') inQuotes = true;
                else if (c == ',') EndCell();
                else if (c == '\r') { /* skip */ }
                else if (c == '\n') EndRow();
                else cell.Append(c);
            }
        }
        // 마지막 줄 처리
        if (cell.Length > 0 || row.Count > 0) EndRow();
        return rows;
    }
}
