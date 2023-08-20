using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.IO;
using System.Drawing;
using FDK;

namespace TJAPlayer3;

internal class CBoxDef
{
    // プロパティ

    public string Genre = "";
    public string Title = "";
    public Color ForeColor = Color.White;
    public Color BackColor = Color.Black;

    // コンストラクタ

    public CBoxDef(string boxdefFileName)
    {
        string[] strs = CJudgeTextEncoding.ReadTextFile(boxdefFileName).Split("\n", StringSplitOptions.RemoveEmptyEntries);
        foreach (var stri in strs)
        {
            var str = stri.TrimStart(' ', '\t');

            if (str.IndexOf(';') != -1)
                str = str.Substring(0, str.IndexOf(';'));

            if (str.Length == 0)
                continue;

            if (str[0] != '#')
                continue;

            try
            {
                char[] ignoreChars = new char[] { ':', ' ', '\t' };

                if (str.StartsWith("#TITLE", StringComparison.OrdinalIgnoreCase))
                {
                    this.Title = str.Substring(6).Trim(ignoreChars);
                }
                else if (str.StartsWith("#GENRE", StringComparison.OrdinalIgnoreCase))
                {
                    this.Genre = str.Substring(6).Trim(ignoreChars);
                }
                else if (str.StartsWith("#FORECOLOR", StringComparison.OrdinalIgnoreCase))
                {
                    this.ForeColor = ColorTranslator.FromHtml(str.Substring(10).Trim(ignoreChars));
                }
                else if (str.StartsWith("#BACKCOLOR", StringComparison.OrdinalIgnoreCase))
                {
                    this.BackColor = ColorTranslator.FromHtml(str.Substring(10).Trim(ignoreChars));
                }
                continue;
            }
            catch (Exception e)
            {
                Trace.TraceError(e.ToString());
                Trace.TraceError("An exception has occurred, but processing continues.");
                continue;
            }
        }
    }
}
