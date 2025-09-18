using System;
using System.Collections.Generic;
using System.Text;

public static class LightCsv
{
    // ����/������ CSV �ļ�: ����ǥ�� ���� �ʵ�, ������ "" �̽�������, �޸� ���� �ʵ� ó��
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
            if (inQuotes) { /*������ CSV ���*/ inQuotes = false; }
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
        // ������ �� ó��
        if (cell.Length > 0 || row.Count > 0) EndRow();
        return rows;
    }
}
